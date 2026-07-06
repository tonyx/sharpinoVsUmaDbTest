module SharpinoVsUma.Commons

open System
open Sharpino
open Sharpino.Cache
open Sharpino.Core
open Sharpino.RabbitMq
open Sharpino.Sample._9
open Sharpino.Sample._9.Balance
open Sharpino.Sample._9.Course
open Sharpino.Sample._9.Item
open Sharpino.Sample._9.Student
open Sharpino.Sample._9.Teacher
open Sharpino.Storage
open DotNetEnv

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System.Threading
open System.Threading.Tasks

    let _ = DotNetEnv.Env.Load() |> ignore
    let connection = Environment.GetEnvironmentVariable("CONNECTION_STRING")

    let pgEventStore:IEventStore<string> = PgStorage.PgEventStore connection
    let memEventStore = MemoryStorage.MemoryStorage()

    let setUp (eventStore: IEventStore<string>) =
        eventStore.Reset Item.Version Item.StorageName
        eventStore.ResetAggregateStream Item.Version Item.StorageName
        eventStore.Reset Course.Version Course.StorageName
        eventStore.ResetAggregateStream Course.Version Course.StorageName
        eventStore.Reset Student.Version Student.StorageName
        eventStore.ResetAggregateStream Student.Version Student.StorageName
        eventStore.Reset Balance.Version Balance.StorageName
        eventStore.ResetAggregateStream Balance.Version Balance.StorageName
        eventStore.Reset Teacher.Version Teacher.StorageName
        eventStore.ResetAggregateStream Teacher.Version Teacher.StorageName
        AggregateCache3.Instance.Clear()
