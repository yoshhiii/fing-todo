module FSharpTodoApi.Todo

open Microsoft.AspNetCore.Http
open Giraffe

[<CLIMutable>]
type Todo = { title: string; complete: bool }

type TodoDb() =
    let mutable allTodos: Todo list = []

    member this.GetAllTodos = fun () -> allTodos

    member this.AddTodo(newTodo: Todo) =
        allTodos <- (newTodo :: allTodos)
        allTodos

type TodoServiceTree = { getTodoDb: unit -> TodoDb }

let getTodosHttpHandler (serviceTree: TodoServiceTree) =
    fun (next: HttpFunc) (ctx: HttpContext) -> json (serviceTree.getTodoDb().GetAllTodos()) next ctx

let createTodoHttpHandler (serviceTree: TodoServiceTree) =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! newTodoJson = ctx.BindJsonAsync<Todo>()
            serviceTree.getTodoDb().AddTodo(newTodoJson) |> ignore
            return! json (newTodoJson) next ctx
        }
