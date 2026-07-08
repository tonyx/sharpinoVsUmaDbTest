
module SharpinoVsUma.Definitions

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System.Threading
open System.Threading.Tasks
open System.Text.Json.Serialization

let jsonOptions =
    JsonFSharpOptions.Default()
        .ToJsonSerializerOptions()