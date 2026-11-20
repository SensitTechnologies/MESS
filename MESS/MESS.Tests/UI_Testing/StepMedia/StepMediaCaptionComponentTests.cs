using Bunit;
using MESS.Blazor.Components.Carousel;
using MESS.Blazor.Components.Pages.Phoebe.WorkInstruction.WorkInstructionNodes.ImageList;
using MESS.Blazor.Components.Pages.Phoebe.WorkInstruction.WorkInstructionNodes.SortableList;
using MESS.Data.Models;
using MESS.Services.Media.WorkInstructions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MESS.Tests.UI_Testing.StepMedia;

/// <summary>
/// bUnit tests for photo captions: the editor's image list (caption inputs) and the
/// production log carousel (caption block below the visible photo).
/// </summary>
public class StepMediaCaptionComponentTests : BunitContext
{
    private readonly Mock<IWorkInstructionImageService> _images = new();
    private (List<StepMediaItem> Primary, List<StepMediaItem> Secondary)? _lastChange;

    public StepMediaCaptionComponentTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(_images.Object);
    }

    private IRenderedComponent<ImageList> RenderImageList(
        List<StepMediaItem> primary, List<StepMediaItem>? secondary = null, bool showBody = false) =>
        Render<ImageList>(p => p
            .Add(c => c.PrimaryMedia, primary)
            .Add(c => c.SecondaryMedia, secondary ?? [])
            .Add(c => c.ShowBody, showBody)
            .Add(c => c.OnMediaChanged, ((List<StepMediaItem> Primary, List<StepMediaItem> Secondary) change) => _lastChange = change));

    private static List<StepMediaItem> ThreeImages() =>
    [
        new() { Path = "a.png", Caption = "Cap A" },
        new() { Path = "b.png" },
        new() { Path = "c.png", Caption = "Cap C" }
    ];

    private static IEnumerable<string?> InputValues(IRenderedComponent<ImageList> cut) =>
        cut.FindAll("input.image-caption-input").Select(i => i.GetAttribute("value")).ToList();

    // ---------------- ImageList ----------------

    [Fact]
    public void ImageList_RendersCaptionInputPerThumbnail()
    {
        var cut = RenderImageList(ThreeImages());

        Assert.Equal(3, cut.FindAll("img").Count);
        Assert.Equal(new string?[] { "Cap A", null, "Cap C" }, InputValues(cut));
        Assert.All(cut.FindAll("input.image-caption-input"),
            i => Assert.Equal(StepMediaItem.CaptionMaxLength.ToString(), i.GetAttribute("maxlength")));
    }

    [Fact]
    public void ImageList_ShowBody_EditsSecondaryCaptions()
    {
        var cut = RenderImageList(ThreeImages(), [new StepMediaItem { Path = "s.png", Caption = "Secondary" }], showBody: true);

        Assert.Equal(new string?[] { "Secondary" }, InputValues(cut));
    }

    [Fact]
    public void ImageList_TypingCaption_UpdatesItemAndRaisesChange()
    {
        var primary = ThreeImages();
        var cut = RenderImageList(primary);

        cut.FindAll("input.image-caption-input")[1].Input("Figure 2");

        Assert.Equal("Figure 2", primary[1].Caption);
        Assert.NotNull(_lastChange);
        Assert.Equal("Figure 2", _lastChange!.Value.Primary[1].Caption);
    }

    [Fact]
    public void ImageList_ClearingCaption_StoresNull()
    {
        var primary = ThreeImages();
        var cut = RenderImageList(primary);

        cut.FindAll("input.image-caption-input")[0].Input("   ");

        Assert.Null(primary[0].Caption);
    }

    [Fact]
    public void ImageList_Remove_DropsImageWithItsCaption()
    {
        var cut = RenderImageList(ThreeImages());

        cut.FindAll("button[title='Remove image']")[0].Click();

        _images.Verify(s => s.DeleteImageFile("a.png"), Times.Once);
        Assert.Equal(["b.png", "c.png"], _lastChange!.Value.Primary.Select(m => m.Path));
        Assert.Equal(new string?[] { null, "Cap C" }, InputValues(cut));
    }

    [Fact]
    public async Task ImageList_Reorder_KeepsCaptionsWithTheirImages()
    {
        var primary = ThreeImages();
        var cut = RenderImageList(primary);
        var sortable = cut.FindComponent<SortableList<StepMediaItem>>();

        await cut.InvokeAsync(() => sortable.Instance.OnUpdate.InvokeAsync((0, 2)));

        Assert.Equal(["b.png", "c.png", "a.png"], primary.Select(m => m.Path));
        Assert.Equal([null, "Cap C", "Cap A"], primary.Select(m => m.Caption));
        Assert.Equal(new string?[] { null, "Cap C", "Cap A" }, InputValues(cut));
    }

    // ---------------- Carousel ----------------

    private IRenderedComponent<Carousel> RenderCarousel(List<string> images, List<string> captions) =>
        Render<Carousel>(p => p
            .Add(c => c.Images, images)
            .Add(c => c.Captions, captions));

    [Fact]
    public void Carousel_RendersCaptionBelowPhoto_NotAsOverlay()
    {
        var cut = RenderCarousel(["a.png", "b.png"], ["Figure 1", "Figure 2"]);

        var caption = cut.Find(".step-photo-caption");
        Assert.Equal("Figure 1", caption.TextContent.Trim());
        Assert.Null(caption.Closest(".carousel"));
        Assert.Empty(cut.FindAll(".carousel-caption"));
        Assert.Single(cut.FindAll(".step-photo-caption"));
    }

    [Fact]
    public void Carousel_CaptionFollowsActiveSlide()
    {
        var cut = RenderCarousel(["a.png", "b.png", "c.png"], ["Figure 1", "", "Figure 3"]);

        cut.Find(".carousel-control-next").Click();
        Assert.Empty(cut.FindAll(".step-photo-caption"));

        cut.Find(".carousel-control-next").Click();
        Assert.Equal("Figure 3", cut.Find(".step-photo-caption").TextContent.Trim());

        cut.Find(".carousel-control-prev").Click();
        cut.Find(".carousel-control-prev").Click();
        Assert.Equal("Figure 1", cut.Find(".step-photo-caption").TextContent.Trim());
    }

    [Fact]
    public void Carousel_NoCaptions_RendersNoCaptionBlock()
    {
        var cut = RenderCarousel(["a.png"], []);

        Assert.Empty(cut.FindAll(".step-photo-caption"));
        Assert.Equal("Slide 1", cut.Find("img").GetAttribute("alt"));
    }

    [Fact]
    public void Carousel_UsesCaptionAsAltText()
    {
        var cut = RenderCarousel(["a.png", "b.png"], ["Figure 1", ""]);

        Assert.Equal(["Figure 1", "Slide 2"], cut.FindAll("img").Select(i => i.GetAttribute("alt")));
    }
}
