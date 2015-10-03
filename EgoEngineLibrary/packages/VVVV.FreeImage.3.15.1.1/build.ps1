nuget pack -NoPackageAnalysis

Move-Item .\VVVV.FreeImage.*.nupkg ..\..\..\LocalNuGet\ -Force