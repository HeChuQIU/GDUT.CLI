open System
open System.Net
open GDUT.Auth
open GDUT.CLI.Utils
open GDUT.ClassSchedule

let getTimestamp () =
    DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()

let exitApplication code = Environment.Exit code

let loginEhall (cookiesSetter: CookieCollection -> unit) =
    let username = askToInputValue "学号"
    let password = askToInputSecretValue "密码"
    let result = Ehall.login username password getTimestamp |> Async.RunSynchronously

    match result with
    | Error errorValue -> printfnWithColor ConsoleColor.Red $"登录失败: {errorValue}"
    | Ok resultValue ->
        printfnWithColor ConsoleColor.Green "登录成功"
        cookiesSetter <| resultValue

let isLoginEhall (cookies: CookieCollection) =
    match Ehall.isLogin cookies with
    | Ok resultValue -> resultValue
    | Error _ -> false

let getSchedule (cookies: CookieCollection) =
    let result = getClassSchedule cookies "202302"

    match result with
    | Error errorValue -> printfnWithColor ConsoleColor.Red $"获取课表失败: {errorValue}"
    | Ok resultValue ->
        printfnWithColor ConsoleColor.Green "获取课表成功:"

        resultValue
        |> Seq.iter (fun x -> printfnWithColor ConsoleColor.Green $"{x}")


let rec main (cookies: CookieCollection) =
    let mutable newCookies = cookies
    let setCookies cookies = newCookies <- cookies

    let baseChoices = [| ("退出程序", (fun _ -> exitApplication 0)) |]

    let login = isLoginEhall cookies

    let choices =
        match login with
        | false -> Array.append baseChoices [| ("登录", (fun _ -> loginEhall setCookies)) |]
        | true ->
            Array.append
                baseChoices
                [| ("注销", (fun _ -> setCookies <| CookieCollection()))
                   ("获取课表", (fun _ -> getSchedule cookies)) |]


    printfn ""

    printfnWithColor
        (if login then ConsoleColor.Green else ConsoleColor.Red)
        $"""当前登录状态: {if login then "已登录" else "未登录"}"""

    choices |> Seq.iteri (fun i (text, _) -> printfn $"%d{i}. %s{text}")
    let input = askToInputValue "序号"

    match Int32.TryParse input with
    | true, i when 0 <= i && i < choices.Length ->
        let _, f = choices[i]
        printfn ""
        f ()
    | _ -> printfnWithColor ConsoleColor.Red "输入无效"

    main newCookies

exit <| (main <| CookieCollection())
