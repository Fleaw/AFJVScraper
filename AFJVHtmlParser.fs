namespace AFJVScrapper

open System
open System.IO
open FSharp.Data
open Models
open Helpers

module AFJVHtmlParser =
    
    [<Literal>]
    let afjvBaseUrl = """https://www.afjv.com"""
    

    let getAFJVStudiosUrl () =
        let htmlDirectory = HtmlDocument.Load("""https://www.afjv.com/annuaire_studio.php?d=100""")
        
        let extractDepartmentAndAFJVUrl (htmlNodeList:HtmlNode []) =
            let department = htmlNodeList.[0].InnerText()
            let afjvStudioUrl = htmlNodeList.[1].AttributeValue "href"
            (department, afjvBaseUrl + afjvStudioUrl)
    
        htmlDirectory.Descendants "div"
        |> Seq.filter (fun d -> d.HasClass """anu_col ombrei br2 """)
        |> Seq.map (fun d -> d.Descendants "a")
        |> Seq.concat
        |> Seq.chunkBySize 2
        |> Seq.map extractDepartmentAndAFJVUrl
        |> Seq.toList
    
    
    let debugProgress index total studioName =
        clearLastConsoleLine()
        Console.Write($"{index + 1}/{total} - {studioName}")
    
    
    let exctractStudioInfoFromUrl directorySize index (department:string, url:string) =
        let htmlStudio = HtmlDocument.Load(url)
        
        let htmlStudioInfos = 
            htmlStudio.Descendants "div"
            |> Seq.filter (fun d -> d.HasId "anu_rig")
            |> Seq.exactlyOne
            |> HtmlNode.elements
            |> List.item 1
            |> HtmlNode.elementsNamed ["div"; "a"; "p"; "strong"]
            |> List.map (fun n ->
                if Seq.length (n.Descendants(["a"; "strong"; "p"])) > 0 then
                    n.Elements().[0]
                else 
                    n
            )
                
        let htmlStudioInfosElementsName () =
            htmlStudioInfos
            |> List.map (fun n -> 
                n.Name()
            )
    
        let contact (htmlNode:HtmlNode) =
            let htmlContact = htmlNode.Elements()
    
            match htmlContact.Length with
            | x when x > 3 ->
                let email = String.concat "@" [htmlContact.[0].ToString(); htmlContact.[2].ToString()]
                let website = htmlContact |> List.rev |> List.head |> HtmlNode.innerText
                (email, website)
            | x when x = 3 ->
                let email = String.concat "@" [htmlContact.[0].ToString(); htmlContact.[2].ToString()]
                (email, "")
            | x when x < 3 ->
                let website = htmlContact |> List.rev |> List.head |> HtmlNode.innerText
                ("", website)
            | _ ->
                ("", "")
    
        let matchHtml =
            match htmlStudioInfosElementsName () with
            | "a"::"strong"::"p"::"p"::[] ->
                (htmlStudioInfos.[0].AttributeValue "href", htmlStudioInfos.[1].InnerText(), htmlStudioInfos.[2].InnerText().Trim(), contact htmlStudioInfos.[3])
            | "a"::"strong"::"p"::[] ->
                (htmlStudioInfos.[0].AttributeValue "href", htmlStudioInfos.[1].InnerText(), htmlStudioInfos.[2].InnerText().Trim(), ("", ""))
            | "strong"::"p"::"p"::[] ->
                ("", htmlStudioInfos.[0].InnerText(), htmlStudioInfos.[1].InnerText().Trim(), contact htmlStudioInfos.[2])
            | "strong"::"p"::[] ->
                ("", htmlStudioInfos.[0].InnerText(), htmlStudioInfos.[1].InnerText().Trim(), ("", ""))
            | _ ->
                ("", "", "", ("", ""))
    
        let googleMapUrl, studioName, phone, studioContact = matchHtml
    
        let address () =
            let addressList =
                htmlStudio.Descendants (fun n -> 
                    n.HasAttribute("property", "og:street-address") ||
                    n.HasAttribute("property", "og:locality") ||
                    n.HasAttribute("property", "og:postal-code") ||
                    n.HasAttribute("property", "og:country-name")
                )
                |> Seq.toList

            let getContentByPropertyFromList attribute (nodeList:HtmlNode list) =
                nodeList
                |> List.tryPick (fun n -> if n.HasAttribute("property", attribute) <> false then Some (n.AttributeValue "content") else None)
                |> Option.defaultValue ""
            
            {
                City = addressList |> getContentByPropertyFromList "og:locality"
                StreetAddress = addressList |> getContentByPropertyFromList "og:street-address"
                PostalCode = addressList |> getContentByPropertyFromList "og:postal-code"
                Country = addressList |> getContentByPropertyFromList "og:country-name"
            }
    
        let studio =
            {
                Name = studioName
                AFJVUrl = url
                Address = address ()
                GoogleMapUrl = googleMapUrl
                Department = department
                Phone = phone
                Email = fst studioContact
                Website = snd studioContact
            }
    
        debugProgress index directorySize studioName
    
        studio
    
    let extractAllStudioFromDirectory directory =
        directory
        |> List.mapi (exctractStudioInfoFromUrl directory.Length)

    let saveStudiosToJson filePath studioList =
        let json =
            studioList
            |> List.map (fun s -> sprintf "%s\n;" (recordToJson s))

        File.AppendAllLines(filePath, json)