module SamlProviderTests.Aws.IamFactoryTests

open System
open System.Threading.Tasks
open NUnit.Framework
open NSubstitute
open SamlProviderTests.Utils
open Cythral.CloudFormation.Resources.Aws
open Amazon.SecurityToken
open Amazon.SecurityToken.Model
open Amazon.Runtime

let toTask computation: Task = Async.StartAsTask computation :> _

module ``Role Arn Was Given`` =
    let mutable iamFactory: IamFactory = null
    let mutable stsFactory: StsFactory = null
    let mutable stsClient: IAmazonSecurityTokenService = null

    let expectedCredentials =
        Credentials(AccessKeyId = "keyId", SecretAccessKey = "secretKey", SessionToken = "sessionToken")

    [<SetUp>]
    let Setup () =
        iamFactory <- IamFactory()

        stsClient <- Substitute.For<IAmazonSecurityTokenService>()
        stsClient.AssumeRoleAsync(Arg.Any<AssumeRoleRequest>())
                 .Returns(AssumeRoleResponse(Credentials = expectedCredentials))
        |> ignore

        stsFactory <- Substitute.For<StsFactory>()
        stsFactory.Create().Returns(stsClient) |> ignore
        SetPrivateField(iamFactory, "stsFactory", stsFactory)

    [<Test>]
    let ``Should create a new Sts Client`` () =
        toTask
        <| async {
            let arn = "arn"

            iamFactory.Create(arn)
            |> Async.AwaitTask
            |> ignore

            stsFactory.Received().Create() |> ignore
           }

    [<Test>]
    let ``Should call assume role`` () =
        toTask
        <| async {
            let arn = "arn"

            iamFactory.Create(arn)
            |> Async.AwaitTask
            |> ignore

            let request =
                Arg.Is(fun (req: AssumeRoleRequest) ->
                    req.RoleArn = arn
                    && req.RoleSessionName = "saml-provider-iam-ops")

            stsClient.Received().AssumeRoleAsync(request)
            |> ignore
           }

    [<Test>]
    let ``Should return an iam client with the proper credentials`` () =
        toTask
        <| async {
            let arn = "arn"
            let! client = iamFactory.Create(arn) |> Async.AwaitTask

            ClientHasCredentials((downcast client: AmazonServiceClient), expectedCredentials)
            |> ignore

            ()
           }

module ``No Role Arn was Given`` =
    let mutable iamFactory: IamFactory = null
    let mutable stsFactory: StsFactory = null
    let mutable stsClient: IAmazonSecurityTokenService = null

    let accessKey = "accessKey"

    let expectedCredentials =
        Credentials(AccessKeyId = " ", SecretAccessKey = " ", SessionToken = " ")

    [<SetUp>]
    let Setup () =
        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-1")
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", " ")
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", " ")
        Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", " ")

        iamFactory <- IamFactory()
        stsClient <- Substitute.For<IAmazonSecurityTokenService>()

        stsFactory <- Substitute.For<StsFactory>()
        stsFactory.Create().Returns(stsClient) |> ignore
        SetPrivateField(iamFactory, "stsFactory", stsFactory)


    [<Test>]
    let ``Should not create an Sts Client`` () =
        toTask
        <| async {
            iamFactory.Create() |> Async.AwaitTask |> ignore
            stsFactory.DidNotReceive().Create() |> ignore
           }

    [<Test>]
    let ``Should return an iam client without credentials`` () =
        toTask
        <| async {
            let! client = iamFactory.Create() |> Async.AwaitTask
            ClientHasCredentials((downcast client: AmazonServiceClient), null)
            ()
           }
