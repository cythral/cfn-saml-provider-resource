module SamlProviderTests.Utils

open System
open System.Reflection

open Amazon.Runtime


let SetPrivateField (target: 't, name: string, value: 'u) =
    let field =
        target.GetType()
              .GetField(name,
                        BindingFlags.Public
                        ||| BindingFlags.NonPublic
                        ||| BindingFlags.Instance
                        ||| BindingFlags.FlattenHierarchy)

    field.SetValue(target, value)

let SetPrivateProperty (target: 't, name: string, value: 'u) =
    let field =
        target.GetType()
              .GetProperty(name,
                           BindingFlags.Public
                           ||| BindingFlags.NonPublic
                           ||| BindingFlags.Instance
                           ||| BindingFlags.FlattenHierarchy)

    field.SetValue(target, value)

let ClientHasCredentials (client: AmazonServiceClient, credentials: AWSCredentials) =
    let prop =
        client.GetType()
              .GetProperty("Credentials",
                           BindingFlags.NonPublic
                           ||| BindingFlags.Instance
                           ||| BindingFlags.FlattenHierarchy)

    let actualCredentials =
        (downcast prop.GetValue(client): AWSCredentials).GetCredentials()

    let mutable givenCredentials: ImmutableCredentials = ImmutableCredentials(" ", " ", " ")

    if not (isNull credentials)
    then givenCredentials <- credentials.GetCredentials()

    printfn "access key %s" actualCredentials.AccessKey

    if (actualCredentials.AccessKey
        <> givenCredentials.AccessKey
        || actualCredentials.SecretKey
           <> givenCredentials.SecretKey
        || actualCredentials.Token <> givenCredentials.Token) then
        failwith "Credentials do not match"
