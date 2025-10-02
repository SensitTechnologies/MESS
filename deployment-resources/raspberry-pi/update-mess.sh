#!/bin/bash
set -e

LOG_TAG="[MESS-UPDATE]"

# Base paths
DEPLOY_DIR="$HOME/MESS/deployment-resources/raspberry-pi"
REPO_DIR="$HOME/MESS"
PROJECT_DIR="$REPO_DIR/MESS"
ENV_FILE="$DEPLOY_DIR/.env"
CONTAINER_NAME="mess-blazor"
OLD_CONTAINER_NAME="${CONTAINER_NAME}-old"

# Data + backup directories live inside deployment folder
DATA_DIR="$DEPLOY_DIR/data"
PERSISTENT_IMAGES_DIR="$DATA_DIR/WorkInstructionImages"
BACKUP_DIR="$DATA_DIR/backups"

mkdir -p "$PERSISTENT_IMAGES_DIR"
mkdir -p "$BACKUP_DIR"

echo "$LOG_TAG [`date`] Backing up WorkInstructionImages..."
BACKUP_FILE="$BACKUP_DIR/WorkInstructionImages-$(date +%F-%H%M).tar.gz"
tar czf "$BACKUP_FILE" -C "$PERSISTENT_IMAGES_DIR" .

# Keep only backups from the last 30 days
echo "$LOG_TAG [`date`] Cleaning up old backups..."
find "$BACKUP_DIR" -type f -name "WorkInstructionImages-*.tar.gz" -mtime +30 -print -delete

cd "$REPO_DIR"
echo "$LOG_TAG [`date`] Checking for new commits..."
git fetch origin main

LOCAL_HASH=$(git rev-parse HEAD)
REMOTE_HASH=$(git rev-parse origin/main)

if [ "$LOCAL_HASH" = "$REMOTE_HASH" ]; then
  echo "$LOG_TAG [`date`] No new commits. Skipping rebuild and restart."
  exit 0
fi

echo "$LOG_TAG [`date`] New commits found. Updating..."
git reset --hard origin/main

cd "$DEPLOY_DIR"

# 1. Stop and rename existing container for rollback
if [ "$(docker ps -aq -f name=^${CONTAINER_NAME}$)" ]; then
  echo "$LOG_TAG [`date`] Stopping and renaming existing container..."
  docker stop "$CONTAINER_NAME" || true

  # If an old rollback container already exists, remove it
  if [ "$(docker ps -aq -f name=^${OLD_CONTAINER_NAME}$)" ]; then
    echo "$LOG_TAG [`date`] Removing stale old container..."
    docker rm -f "$OLD_CONTAINER_NAME"
  fi

  docker rename "$CONTAINER_NAME" "$OLD_CONTAINER_NAME"
fi

# 2. Remove any leftover container with the same name
if [ "$(docker ps -aq -f name=^${CONTAINER_NAME}$)" ]; then
  echo "$LOG_TAG [`date`] Removing leftover container with same name..."
  docker rm -f "$CONTAINER_NAME"
fi

# 3. Build new Docker image
echo "$LOG_TAG [`date`] Building Docker image..."
if ! docker build -t "$CONTAINER_NAME:latest" "$DEPLOY_DIR"; then
  echo "$LOG_TAG [`date`] Docker build failed! Rolling back old container..."
  if [ "$(docker ps -aq -f name=^${OLD_CONTAINER_NAME}$)" ]; then
    docker rename "$OLD_CONTAINER_NAME" "$CONTAINER_NAME"
    docker start "$CONTAINER_NAME"
  fi
  exit 1
fi

# 4. Start new container
echo "$LOG_TAG [`date`] Starting new container..."
if ! docker run -d \
  --restart unless-stopped \
  -p 80:8080 \
  --name "$CONTAINER_NAME" \
  --env-file "$ENV_FILE" \
  -v "$PERSISTENT_IMAGES_DIR:/app/wwwroot/WorkInstructionImages" \
  "$CONTAINER_NAME:latest"; then
  echo "$LOG_TAG [`date`] Docker run failed! Rolling back old container..."
  docker rm -f "$CONTAINER_NAME" || true
  if [ "$(docker ps -aq -f name=^${OLD_CONTAINER_NAME}$)" ]; then
    docker rename "$OLD_CONTAINER_NAME" "$CONTAINER_NAME"
    docker start "$CONTAINER_NAME"
  fi
  exit 1
fi

# 5. Remove old container if new one succeeded
if [ "$(docker ps -aq -f name=^${OLD_CONTAINER_NAME}$)" ]; then
  echo "$LOG_TAG [`date`] Removing old container..."
  docker rm "$OLD_CONTAINER_NAME"
fi

# 6. Clean up old images
echo "$LOG_TAG [`date`] Cleaning up unused Docker images..."
docker image prune -f

echo "$LOG_TAG [`date`] Update complete!"