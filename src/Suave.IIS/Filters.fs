module Suave.IIS.Filters

/// IIS wrapper for Suave.Filters.path function
let path (args:string []) p =
    match args |> Configuration.parseSetup with
    | Some({Path = Some setupPath}) -> Suave.Filters.path <| sprintf "/%s%s" setupPath p
    | _ -> Suave.Filters.path p

/// IIS wrapper for Suave.Filters.pathScan function    
let pathScan (args:string []) (pf:PrintfFormat<'a,'b,'c,'d,'t>)=
    match args |> Configuration.parseSetup with
    | Some({Path = Some setupPath}) -> 
        let newValue = "/" + setupPath + pf.Value
        Suave.Filters.pathScan (PrintfFormat<'a,'b,'c,'d,'t>(newValue))
    | _ -> Suave.Filters.pathScan pf
