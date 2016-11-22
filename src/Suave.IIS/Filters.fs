module Suave.IIS.Filters

/// IIS wrapper for Suave.Filters.path function
let path (args:string []) p =
    match args |> Configuration.parseSetup with
    | Some(setup)       -> Suave.Filters.path <| sprintf "/%s%s" setup.Path p
    | None              -> Suave.Filters.path p

/// IIS wrapper for Suave.Filters.pathScan function    
let pathScan (args:string []) (pf:PrintfFormat<'a,'b,'c,'d,'t>)=
    match args |> Configuration.parseSetup with
    | Some(setup)   -> 
        let newValue = "/" + setup.Path + pf.Value
        Suave.Filters.pathScan (PrintfFormat<'a,'b,'c,'d,'t>(newValue))
    | None              -> Suave.Filters.pathScan pf