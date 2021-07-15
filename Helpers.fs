namespace AFJVScrapper

open System
open System.IO
open FSharp.Json
open Models

module Helpers =
    
    let upcastToObj (a:'a) =
        a :> obj

    let clearLastConsoleLine () =
        Console.Write("\r" + String(' ', Console.WindowWidth) + "\r")

    let recordToJson record =
        Json.serialize record

    let jsonToStudio json =
        Json.deserialize<Studio> json

    let fileToString filePath =
        File.ReadAllText filePath

    let formatUrl (url:string) =
        match url.Substring(0, 4) with
        | "http" -> url
        | _ -> sprintf "http://%s" url