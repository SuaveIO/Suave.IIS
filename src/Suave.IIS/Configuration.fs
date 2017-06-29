module Suave.IIS.Configuration

open Suave
open System
open System.Net

type Setup = {
    Port: uint16
    Path: string option
}

let tryGetPortFromEnv () =
    match Environment.GetEnvironmentVariable "ASPNETCORE_PORT" with
    | null -> None
    | port -> Some port

let tryParsePort port =
    match UInt16.TryParse port with
    | true, port -> Some port
    | false, _ -> None

let tryReadPort port =
    match tryParsePort port with
    | Some port -> Some port
    | None ->
        tryGetPortFromEnv ()
        |> Option.bind tryParsePort

let parsePort port =
    match tryReadPort port with
    | Some port -> port
    | None -> failwithf "Please provide a valid port as the first argument: %s" port

/// Parse configuration setup from command line args
let parseSetup (args:string []) =
    match args with
    | [| port; path |] -> { Port = parsePort port; Path = Some path } |> Some
    | [| port |] -> { Port = parsePort port; Path = None } |> Some
    | _ -> None

/// Set Suave port based on command line args setup
let withPort (args:string []) config =
    match args |> parseSetup with
    | Some(setup) -> { config with bindings=[HttpBinding.create HTTP IPAddress.Any setup.Port]}
    | None -> config
