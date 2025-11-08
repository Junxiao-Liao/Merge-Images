namespace MergeImages.Core.Tests

open System
open Xunit
open MergeImages.Core.Types
open MergeImages.Core.Validation

module ValidationTests =

    [<Fact>]
    let ``validateMergeRequest returns error when image list is empty`` () =
        let req = {
            Images = []
            Options = {
                Direction = MergeDirection.Vertical
                Spacing = 0
                Background = BackgroundFill.Transparent
            }
        }
        match validateMergeRequest req with
        | Ok _ -> Assert.Fail("Expected error for empty image list")
        | Result.Error errs -> Assert.True(errs |> List.contains ValidationError.EmptyImageList)

    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(5)>]
    let ``validateMergeRequest passes for non-empty list and non-negative spacing`` spacing =
        let req = {
            Images = [ { Id = Guid.NewGuid(); FilePath = "a.png"; Order = 0 } ]
            Options = {
                Direction = MergeDirection.Horizontal
                Spacing = spacing
                Background = BackgroundFill.Solid { R=255uy; G=255uy; B=255uy; A=255uy }
            }
        }
        match validateMergeRequest req with
        | Ok _ -> Assert.True(true)
        | Result.Error errs -> Assert.Fail($"Unexpected errors: {errs}")
