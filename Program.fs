namespace AFJVScrapper

open System
open Helpers
open AFJVHtmlParser
open ExcelCreator

module Program =

    let jsonToStudioList (json:string) =
        json.Split ";"
        |> Array.rev
        |> Array.skip 1
        |> Array.rev
        |> Array.map jsonToStudio
        |> Array.toList

    [<EntryPoint>]
    let main argv =

        (*
        printfn "Reading AFJV annuaire..."
        let afjvAnnuaire = getAFJVAnnuaireUrls ()
        printfn "Done."
        printfn "Annuaire size : %i" (afjvAnnuaire |> List.length)

        printfn "Extracting studio info from annuaire..."
        let studios = extractAllStudioFromAnnuaire afjvAnnuaire
        clearLastConsoleLine ()
        printfn "Done."

        printfn @"Saving studios to json file..."
        saveStudiosToJson "Studios.txt" studios
        printfn "Done."
        *)
        
        let studios = 
            fileToString "Studios.txt"
            |> jsonToStudioList

        printfn "Importing studios to excel..."
        importStudiosToExcel studios
        printfn "Done."

        printfn "Press any key to close."

        Console.ReadLine() |> ignore

        0