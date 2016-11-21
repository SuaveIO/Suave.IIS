module Suave.IIS.Configuration

open Suave
open System.Net

type Setup = {
    Port: uint16
    Path: string
}

/// Parse configuration setup from command line args
let parseSetup (args:string []) =
    match args |> Array.length with
    | x when x >= 2 -> { Port = uint16 args.[0]; Path = args.[1] } |> Some
    | _  -> None

/// Set Suave port based on command line args setup
let withPort (args:string []) config =
    match args |> parseSetup with
    | Some(setup) -> { config with bindings=[HttpBinding.mk HTTP IPAddress.Any setup.Port]}
    | None -> config
