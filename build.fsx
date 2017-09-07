// include Fake lib
#I "packages/FAKE/tools/"

#r "FakeLib.dll"

open System
open System.IO
open Fake 
open Fake.AssemblyInfoFile
open System.Diagnostics
open Fake.Testing

let appSrc = "src/Suave.IIS"
let testAppSrc = "tests/Suave.IIS.TestApp"

// Read release notes & version info from RELEASE_NOTES.md
let release = File.ReadLines "RELEASE_NOTES.md" |> ReleaseNotesHelper.parseReleaseNotes

Target "Nuget" <| fun () ->
    let toNotes = List.map (fun x -> x + System.Environment.NewLine) >> List.fold (+) ""
    let args = 
        [
            "PackageId=\"Suave.IIS\""
            "Title=\"Suave.IIS\""
            "Description=\"Set of helper functions for smooth running Suave web server on Internet Information Services (IIS)\""
            sprintf "PackageVersion=\"%s\"" release.NugetVersion
            sprintf "PackageReleaseNotes=\"%s\"" (release.Notes |> toNotes)
            "PackageLicenseUrl=\"https://github.com/SuaveIO/suave/blob/master/COPYING\""
            "PackageProjectUrl=\"https://github.com/SuaveIO/Suave.IIS\""
            "PackageIconUrl=\"https://raw.githubusercontent.com/SuaveIO/resources/master/images/head_trans.png\""
            "PackageTags=\"Suave IIS Internet Information Services\""
            "Copyright=\"Roman Provazník - 2017\""
            "Authors=\"Roman Provazník\""
        ] |> List.map (fun x -> "/p:" + x)


    Fake.DotNetCli.Pack (fun p -> { p with Project = appSrc; OutputPath = "../../nuget"; AdditionalArgs = args })

Target "BuildApp" (fun _ ->
    Fake.DotNetCli.Build (fun p -> { p with Project = appSrc; Configuration = "Debug";})
)

Target "BuildTestApp" (fun _ ->
    Fake.DotNetCli.Publish (fun p -> { p with Project = testAppSrc; Configuration = "Release" })
)

Target "Clean" (fun _ -> 
    DeleteDir (appSrc + "/bin")
    DeleteDir (appSrc + "/obj")
    DeleteDir "nuget" 
)

"Clean" ==>  "Nuget"

// start build
RunTargetOrDefault "BuildApp"