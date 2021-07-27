namespace AFJVScrapper

open System
open System.IO
open Helpers
open AFJVHtmlParser
open ExcelCreator

module Program =

    module Excel =
    
        let importStudiosToExcel studios =
            printfn "\nImporting studios to excel..."
            importStudiosToExcel studios
            printfn "Done."

    module LoadAndSave =
        
        let jsonToStudioList (json:string) =
            json.Split ";"
            |> Array.rev
            |> Array.skip 1
            |> Array.rev
            |> Array.map jsonToStudio
            |> Array.toList

        let saveStudiosToJsonFile studios =
            printfn @"Saving studios to json file..."
            saveStudiosToJson "Studios.txt" studios
            printfn "Done."
            studios
        
        let loadStudiosFromJsonFile filePath =
            fileToString filePath
            |> jsonToStudioList

    module Parser =

        let readStudioDirectory () =
            printfn "\nReading AFJV annuaire..."
            let studioDirectory = getAFJVStudiosUrl ()
            printfn "Done."
            printfn "Annuaire size : %i" (studioDirectory |> List.length)
            studioDirectory

        let extractStudiosInfoFromDirectory directory =
            printfn "\nExtracting studios info from directory..."
            let studios = extractAllStudioFromDirectory directory
            clearLastConsoleLine ()
            printfn "Done."
            studios

    module ConsoleMenu =
        
        let printMenuOption () =
            printfn "1 - Read and import AFJV studio directory into excel\n2 - Import studios into excel from file"

        let readFilePath () =
            let filePath = Console.ReadLine()
            match File.Exists filePath with
            | true -> Some filePath
            | false -> None
    
        let matchKeyToMenu key =
            match key with
            | ConsoleKey.NumPad1 ->
                Parser.readStudioDirectory  ()
                |> Parser.extractStudiosInfoFromDirectory
                |> LoadAndSave.saveStudiosToJsonFile
                |> Excel.importStudiosToExcel
                true
            | ConsoleKey.NumPad2 ->
                printf "\nFile path : "
                match readFilePath () with
                | Some filePath ->
                    LoadAndSave.loadStudiosFromJsonFile "Studios.txt"
                    |> Excel.importStudiosToExcel
                    true
                | None -> 
                    printfn "\nError: Specified file does not exist.\n"
                    false
            | _ ->
                printfn "\nError: Invalid menu entry\n"
                false

        let rec printMenu () =
            printMenuOption ()

            let result =
                Console.ReadKey().Key
                |> matchKeyToMenu

            match result with
            | false -> printMenu ()
            | true -> ()

    [<EntryPoint>]
    let main argv =
            
        ConsoleMenu.printMenu ()

        printfn "\nEnd of the program. Press any key to close."

        Console.ReadLine() |> ignore

        0