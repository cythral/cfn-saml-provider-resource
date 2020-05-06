module SamlProviderTests.Aws.S3GetObjectFacadeTests

open System
open System.Text
open System.IO
open System.Runtime.Serialization
open System.Threading.Tasks
open NUnit.Framework
open Cythral.CloudFormation.Resources.Aws
open SamlProviderTests.Utils
open NSubstitute
open FsUnit
open Amazon
open Amazon.S3
open Amazon.S3.Model
open Amazon.S3.Util
open Amazon.SecurityToken

let toTask computation: Task = Async.StartAsTask computation :> _

module ``GetObject Success Flow`` =
    let mutable s3Client: IAmazonS3 = null
    let mutable s3Factory: S3Factory = null
    let mutable stsFactory: StsFactory = null
    let mutable stsClient: IAmazonSecurityTokenService = null
    let mutable s3GetObjectFacade: S3GetObjectFacade = null
    let mutable s3UriFactory: S3UriFactory = null

    let metadataDoc = "doc"
    let bucket = "bucket"
    let key = "key"
    let region = RegionEndpoint.USEast1
    let mutable s3Uri: AmazonS3Uri = null
    let uri = "uri"
    let downloaderArn = "arn"

    [<SetUp>]
    let Setup = s3GetObjectFacade <- S3GetObjectFacade()

    [<SetUp>]
    let SetupS3 () =
        s3Client <- Substitute.For<IAmazonS3>()

        let bytes = Encoding.UTF8.GetBytes(metadataDoc)
        let stream = new MemoryStream(bytes)

        let response =
            new GetObjectResponse(ResponseStream = stream)

        s3Client.GetObjectAsync(Arg.Any<GetObjectRequest>()).Returns(response)
        |> ignore

        s3Factory <- Substitute.For<S3Factory>()
        SetPrivateField(s3GetObjectFacade, "s3Factory", s3Factory)

        s3Factory.Create(Arg.Any<string>()).Returns(s3Client)
        |> ignore
        ()

    [<SetUp>]
    let SetupS3Uri () =
        s3Uri <- downcast FormatterServices.GetUninitializedObject(typeof<AmazonS3Uri>)
        SetPrivateProperty(s3Uri, "Bucket", bucket)
        SetPrivateProperty(s3Uri, "Key", key)
        SetPrivateProperty(s3Uri, "Region", region)

        s3UriFactory <- Substitute.For<S3UriFactory>()
        SetPrivateField(s3GetObjectFacade, "s3UriFactory", s3UriFactory)

        s3UriFactory.Create(Arg.Any<string>()).Returns<AmazonS3Uri>(s3Uri)
        |> ignore

    [<Test>]
    let ``An s3 client is created with the requested role arn`` () =
        toTask
        <| async {
            s3GetObjectFacade.GetObject(uri, downloaderArn)
            |> Async.AwaitTask
            |> ignore

            s3Factory.Received().Create(Arg.Is<string>(downloaderArn))
            |> ignore
            ()
           }

    [<Test>]
    let ``An s3 uri is created with the requested metadata document location`` () =
        toTask
        <| async {
            s3GetObjectFacade.GetObject(uri, downloaderArn)
            |> Async.AwaitTask
            |> ignore

            s3UriFactory.Received().Create(Arg.Is<string>(uri))
            |> ignore
            ()
           }

    [<Test>]
    let ``The metadata doc is downloaded from S3`` () =
        toTask
        <| async {
            s3GetObjectFacade.GetObject(uri, downloaderArn)
            |> Async.AwaitTask
            |> ignore

            let request =
                fun (req: GetObjectRequest) -> req.BucketName = bucket && req.Key = key

            s3Client.Received().GetObjectAsync(Arg.Is<GetObjectRequest>(request))
            |> ignore
            ()
           }

    [<Test>]
    let ``The metadata doc is returned`` () =
        toTask
        <| async {
            let! doc =
                s3GetObjectFacade.GetObject(uri, downloaderArn)
                |> Async.AwaitTask
            doc |> should equal metadataDoc
           }
