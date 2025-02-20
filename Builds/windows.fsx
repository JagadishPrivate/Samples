#r @"FAKE.3.26.1/tools/FakeLib.dll"

open Fake
open Fake
open Fake.XamarinHelper
open System
open System.Collections.Generic
open System.IO
open System.Linq
open Fake.AssemblyInfoFile

let configuration = "Release"
let architecture = "Any CPU"
let rootFolder = "../"
let WaveToolDirectory = "WaveEngine.WindowsTools"

let getFolder solutionFile= Path.GetDirectoryName(solutionFile)

let Exec command args =
    let result = Shell.Exec(command, args)

    if result <> 0 then failwithf "%s exited with error %d" command result

let RestorePackages solutionFile =
    RestoreMSSolutionPackages (fun p -> 
        { p with
            Sources = "https://www.myget.org/F/waveengine-nightly/api/v2" :: p.Sources
            Retries = 5 
            OutputPath = Path.Combine(getFolder solutionFile, "packages") }) solutionFile

type status =
    | Success
    | Failed

type sampleReport = 
    {
        Result : status
        Path : string
    }

let items = new List<sampleReport>()

let processResults (path : string) (flag : bool) =
    let report : sampleReport = 
        {
            Result = if flag then status.Success else status.Failed
            Path = path
        }

    items.Add(report)

let mutable OkProjects = 0;

let printReport (l : List<sampleReport>) =
    printfn ""
    traceImportant "---------------------------------------------------------------------"
    traceImportant "Samples Report:"
    traceImportant "---------------------------------------------------------------------"
    l |> Seq.iteri (fun index item ->
        if l.[index].Result = status.Success then
            trace (index.ToString() + "-    Success " + l.[index].Path)
            OkProjects <- OkProjects + 1
        else
            traceError (index.ToString() + "-    Failed " + l.[index].Path))

    printfn ""
    printfn "   Projects success: %i / %i" OkProjects l.Count
    traceImportant "---------------------------------------------------------------------"
    printfn ""

let buildSample (platform: string, configuration : string, architecture : string, sample : string) = 
    match platform with
    | "Windows" -> MSBuild null "Build" [("Configuration", configuration); ("Platform", architecture)] [sample] |> ignore
    | "Linux" -> Exec "xbuild" ("/p:Configuration=" + configuration + " " + sample)
    | "MacOS" -> Exec "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool" ("-v build -t:Build -c:" + configuration + " " + sample)
    | _-> ()

let buildsamples(platform: string) =
    for sample in Directory.GetFiles(rootFolder, ("*" + platform + ".sln"), SearchOption.AllDirectories) do
        traceImportant ("Project " + sample)

        let mutable flag = true

        try
            traceImportant ("restoring..")
            RestorePackages sample

            traceImportant ("Building...")
            

            buildSample (platform, configuration, architecture, sample)
        with
            | _ -> flag <- false
        
        processResults sample flag

    printReport items

Target "environment-var" (fun () ->
    let variablePath = System.IO.Path.GetFullPath("WaveEngine.WindowsTools/");
    trace variablePath
    let variableName = "WaveEngine"

    setEnvironVar variableName variablePath
    trace "Environment Variable created"
)

Target "restore-windowstools" (fun() ->
    traceImportant "Clear tools directory"
    DeleteDirs [WaveToolDirectory]

    traceImportant "Get WaveEngine.WindowsTools nuget packages"
    let nugetArgs = " install " + WaveToolDirectory + " -ExcludeVersion -ConfigFile NuGet\NuGet.config"
    trace nugetArgs
    Exec "NuGet/nuget.exe" nugetArgs

    traceImportant "Generate waveengine installation path"
    let target = WaveToolDirectory + "/v2.0/Tools/VisualEditor/"
    !! (WaveToolDirectory + "/tools/*.*")
        |> CopyFiles target
)

Target "update-nightlypackages" (fun() ->
    traceImportant "Update to nightly nuget packages"
    Exec "WaveTools/UpdateToNightlyPackages.exe" rootFolder
)

Target "windows-samples" (fun () ->
    buildsamples("Windows")
)

Target "macos-samples" (fun () ->
    buildsamples("MacOS")
)

Target "linux-samples" (fun () ->
    buildsamples("Linux")
)

"restore-windowstools"
    ==> "environment-var"
    ==> "update-nightlypackages"
    ==> "windows-samples"
 