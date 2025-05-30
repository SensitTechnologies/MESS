﻿namespace MESS.Services.ProductionLog;
using Data.Models;
/// <summary>
/// Interface for managing production log events and related details.
/// </summary>
public interface IProductionLogEventService
{
    /// <summary>
    /// Gets or sets the current production log.
    /// </summary>
    public ProductionLog? CurrentProductionLog { get; set; }

    /// <summary>
    /// Gets or sets the name of the current product.
    /// </summary>
    public string CurrentProductName { get; set; }

    /// <summary>
    /// Gets or sets the name of the current workstation.
    /// </summary>
    public string CurrentWorkStationName { get; set; }

    /// <summary>
    /// Gets or sets the name of the current work instruction.
    /// </summary>
    public string CurrentWorkInstructionName { get; set; }

    /// <summary>
    /// Gets or sets the name of the current line operator.
    /// </summary>
    public string CurrentLineOperatorName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the production log is saved.
    /// </summary>
    public bool IsSaved { get; set; }

    /// <summary>
    /// Event triggered when the production log changes.
    /// </summary>
    public event Action? ProductionLogEventChanged;

    /// <summary>
    /// Event triggered when product details change.
    /// </summary>
    public event Action? ProductDetailsChanged;

    /// <summary>
    /// Event triggered when work instruction details change.
    /// </summary>
    public event Action? WorkInstructionDetailsChanged;

    /// <summary>
    /// Event triggered when workstation details change.
    /// </summary>
    public event Action? WorkStationDetailsChanged;

    /// <summary>
    /// Event triggered when line operator details change.
    /// </summary>
    public event Action? LineOperatorDetailsChanged;

    /// <summary>
    /// Event triggered to handle auto-save functionality.
    /// </summary>
    public event Func<ProductionLog, Task>? AutoSaveTriggered;

    /// <summary>
    /// Disables the auto-save functionality.
    /// </summary>
    public void DisableAutoSave();

    /// <summary>
    /// Enables the auto-save functionality.
    /// </summary>
    public void EnableAutoSave();

    /// <summary>
    /// Handles changes made to the production log.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ChangeMadeToProductionLog();

    /// <summary>
    /// Sets the current product name.
    /// </summary>
    /// <param name="productName">The name of the product to set.</param>
    public void SetCurrentProductName(string productName);

    /// <summary>
    /// Sets the current workstation name.
    /// </summary>
    /// <param name="workStationName">The name of the workstation to set.</param>
    public void SetCurrentWorkStationName(string workStationName);

    /// <summary>
    /// Sets the current work instruction name.
    /// </summary>
    /// <param name="workInstructionName">The name of the work instruction to set.</param>
    public void SetCurrentWorkInstructionName(string workInstructionName);

    /// <summary>
    /// Sets the current line operator name.
    /// </summary>
    /// <param name="lineOperatorName">The name of the line operator to set.</param>
    public void SetCurrentLineOperatorName(string lineOperatorName);

    /// <summary>
    /// Gets the current production log.
    /// </summary>
    /// <returns>The current production log, or null if none is set.</returns>
    public ProductionLog? GetCurrentProductionLog();

    /// <summary>
    /// Sets the current production log.
    /// </summary>
    /// <param name="productionLog">The production log to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SetCurrentProductionLog(ProductionLog productionLog);
}