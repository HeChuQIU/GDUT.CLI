module GDUT.CLI.Utils

open System

let printfnWithColor color text =
    Console.ForegroundColor <- color
    printfn text
    Console.ResetColor()

let rec askToInputValue name =
    printfn $"请输入%s{name}:"
    let input = Console.ReadLine()
    match input with
    | _ when String.IsNullOrEmpty input ->
        printfnWithColor ConsoleColor.Red $"%s{name} 不能为空"
        askToInputValue name
    | _ -> input

let rec private readSecureValuePaddingWith (p: string) (s: string) =
    let key = Console.ReadKey(true)

    match key.Key with
    | ConsoleKey.Enter ->
        printfn ""
        s
    | ConsoleKey.Backspace ->
        let sn =
            if s.Length > 0 then
                let spaces = String.replicate p.Length " "
                Console.Write($"\b{spaces}\b")
                s[0 .. s.Length - 2]
            else
                s

        readSecureValuePaddingWith p sn
    | _ ->
        p |> Console.Write
        let sn = $"%s{s}%c{key.KeyChar}"
        readSecureValuePaddingWith p sn

let private readSecureValue (s: string) = readSecureValuePaddingWith "*" s

let rec askToInputSecretValue name =
    printfn $"请输入{name}： "

    let input = readSecureValuePaddingWith "" String.Empty

    match input with
    | _ when String.IsNullOrEmpty input ->
        printfnWithColor ConsoleColor.Red $"{name} 不能为空"
        askToInputSecretValue name
    | _ -> input
