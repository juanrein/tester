module Tester
(*
Composition pattern
Runs test suite of hierarchical tests and prints their results
*)

// dotnet fsi

open System.Net.Http
open System

type HttpTest =
| Root of Uri:string * Tests: List<HttpTest>
| Method of Method: HttpMethod * Tests: List<HttpTest>
| Endpoint of Uri: string * Tests: List<HttpTest>
| Test of Parameters: List<(string * string)> * Expected: string

type TestBuilder = {
    Method: HttpMethod option
    Uri: string option
    Expected: string option
    Parameters: List<string * string> option
    Client: HttpClient
}

exception IncompleteError of string



type TestResult = {
    Actual: string
    Expected: string
    Status: Net.HttpStatusCode
    Url: Uri
}

//Send http request 
let sendAsync (builder: TestBuilder) =
    match builder with 
    | {
        Uri = Some uri; 
        Method = Some method; 
        Client = client; 
        Parameters = Some parameters; 
        Expected = Some expected
      } -> 
        let request = new HttpRequestMessage()
        request.Method <- method
        //TODO: escape
        let query = 
            parameters 
            |> List.map (fun (a,b) -> a + "=" + b)
            |> List.fold (fun acc elem -> acc + "&" + elem) ""
        let finalUrl = Uri (uri + "?" + query)
        request.RequestUri <- finalUrl
        async {
            let! response = client.SendAsync(request) |> Async.AwaitTask
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let code = response.StatusCode

            return {
                Expected = expected
                Actual = content
                Status = code
                Url = finalUrl
            }
        } |> Async.Catch

    | _ -> raise (IncompleteError($"Incomplete request {builder}"))

//Transforms testsuite into list of asynchonous tasks
let rec buildTests (builder: TestBuilder) (test:HttpTest) =
    let setOrAppend (a: string option) (b: string) =
        match a with
        | Some(p) -> Some (p + "/" + b)
        | None -> Some b
    
    match test with
    | Root(url, tests) -> 
        let path2 = setOrAppend builder.Uri url
        tests
        |> List.map (buildTests {builder with Uri = path2}) 
        |> List.concat

    | Method(method, tests) ->
        tests 
        |> List.map (buildTests {builder with Method = Some method})
        |> List.concat

    | Endpoint(url, tests) ->
        let path2 = setOrAppend builder.Uri url
        tests 
        |> List.map (buildTests {builder with Uri = path2})
        |> List.concat
   
    | Test(parameters, expected) ->
        let res = sendAsync {builder with Parameters = Some parameters; Expected = Some expected}
        [res]

let printResults (results: Choice<TestResult, exn> []) =
    for result in results do
        match result with
        | Choice1Of2 value -> printfn "%A" value
        | Choice2Of2 e -> printfn "%A" e.Message

