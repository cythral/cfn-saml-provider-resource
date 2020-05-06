module SamlProviderTests.Core

open System.Threading.Tasks
open NUnit.Framework
open Cythral.CloudFormation.Resources
open Cythral.CloudFormation.Resources.Aws
open Cythral.CloudFormation.CustomResource.Core
open SamlProviderTests.Utils
open NSubstitute
open FsUnit
open Amazon
open Amazon.S3
open Amazon.S3.Util
open Amazon.IdentityManagement
open Amazon.IdentityManagement.Model

let toTask computation: Task = Async.StartAsTask computation :> _

module Create =
    let mutable samlProvider: SamlProvider = null
    let mutable iamFactory: IamFactory = null
    let mutable s3Factory: S3Factory = null
    let mutable s3Client: IAmazonS3 = null
    let mutable s3UriFactory: S3UriFactory = null
    let mutable s3GetObjectFacadeFactory: S3GetObjectFacadeFactory = null
    let mutable s3GetObjectFacade: S3GetObjectFacade = null
    let mutable iamClient: IAmazonIdentityManagementService = null

    let bucket = "bucket"
    let key = "key"
    let region = RegionEndpoint.USEast1
    let mutable s3Uri: AmazonS3Uri = null

    let metadataDoc = "doc"
    let providerArn = "providerArn"

    [<SetUp>]
    let Setup () = samlProvider <- SamlProvider()

    [<SetUp>]
    let SetupS3GetObjectFacade () =
        s3GetObjectFacade <- Substitute.For<S3GetObjectFacade>()
        s3GetObjectFacade.GetObject(Arg.Any<string>(), Arg.Any<string>()).Returns(metadataDoc)
        |> ignore

        s3GetObjectFacadeFactory <- Substitute.For<S3GetObjectFacadeFactory>()
        SetPrivateField(samlProvider, "s3GetObjectFacadeFactory", s3GetObjectFacadeFactory)

        s3GetObjectFacadeFactory.Create().Returns(s3GetObjectFacade)
        |> ignore


    [<SetUp>]
    let SetupIAM () =
        iamClient <- Substitute.For<IAmazonIdentityManagementService>()

        let response =
            CreateSAMLProviderResponse(SAMLProviderArn = providerArn)

        iamClient.CreateSAMLProviderAsync(Arg.Any<CreateSAMLProviderRequest>()).Returns(response)
        |> ignore

        iamFactory <- Substitute.For<IamFactory>()
        SetPrivateField(samlProvider, "iamFactory", iamFactory)

        iamFactory.Create(Arg.Any<string>()).Returns(iamClient)
        |> ignore

    [<Test>]
    let ``An iam client is created with the requested role arn`` () =
        toTask
        <| async {
            let iamRole = "iamRoleArn"

            samlProvider.Request <- new Request<SamlProvider.Properties>()
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties(CreatorRoleArn = iamRole)
            samlProvider.Create() |> Async.AwaitTask |> ignore

            iamFactory.Received().Create(Arg.Is<string>(iamRole))
            |> ignore
            ()
           }

    [<Test>]
    let ``An s3GetObjectFacade is created`` () =
        toTask
        <| async {
            let iamRole = "iamRoleArn"

            samlProvider.Request <- new Request<SamlProvider.Properties>()
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties(CreatorRoleArn = iamRole)
            samlProvider.Create() |> Async.AwaitTask |> ignore

            s3GetObjectFacadeFactory.Received().Create()
            |> ignore
            ()
           }


    [<Test>]
    let ``The metadata document is downloaded from s3`` () =
        toTask
        <| async {
            let downloaderArn = "downloaderArn"
            let samlLocation = "samlLocation"

            let props =
                SamlProvider.Properties(DownloaderRoleArn = downloaderArn, SamlMetadataDocumentLocation = samlLocation)

            samlProvider.Request <- new Request<SamlProvider.Properties>(ResourceProperties = props)

            samlProvider.Create() |> Async.AwaitTask |> ignore
            s3GetObjectFacade.Received().GetObject(Arg.Is<string>(samlLocation), Arg.Is<string>(downloaderArn))
            |> ignore
            ()
           }

    [<Test>]
    let ``The saml provider is created with the provided name and metadata doc`` () =
        toTask
        <| async {
            let name = "name"
            samlProvider.Request <- new Request<SamlProvider.Properties>()
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties(Name = name)
            samlProvider.Create() |> Async.AwaitTask |> ignore

            let request =
                fun (req: CreateSAMLProviderRequest) ->
                    req.SAMLMetadataDocument = metadataDoc
                    && req.Name = name

            iamClient.Received().CreateSAMLProviderAsync(Arg.Is<CreateSAMLProviderRequest>(request))
            |> ignore
            ()
           }

    [<Test>]
    let ``Create responds with the provider arn`` () =
        toTask
        <| async {
            let name = "name"
            samlProvider.Request <- new Request<SamlProvider.Properties>()
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties(Name = name)
            let! response = samlProvider.Create() |> Async.AwaitTask

            response.PhysicalResourceId
            |> should equal providerArn

            ()
           }

module Update =
    let mutable samlProvider: SamlProvider = null
    let mutable iamFactory: IamFactory = null
    let mutable s3Factory: S3Factory = null
    let mutable s3Client: IAmazonS3 = null
    let mutable s3UriFactory: S3UriFactory = null
    let mutable s3GetObjectFacadeFactory: S3GetObjectFacadeFactory = null
    let mutable s3GetObjectFacade: S3GetObjectFacade = null
    let mutable iamClient: IAmazonIdentityManagementService = null

    let bucket = "bucket"
    let key = "key"
    let region = RegionEndpoint.USEast1
    let mutable s3Uri: AmazonS3Uri = null

    let metadataDoc = "doc"
    let providerArn = "providerArn"

    [<SetUp>]
    let SetUp = samlProvider <- SamlProvider()

    [<SetUp>]
    let SetupS3GetObjectFacade () =
        s3GetObjectFacade <- Substitute.For<S3GetObjectFacade>()
        s3GetObjectFacade.GetObject(Arg.Any<string>(), Arg.Any<string>()).Returns(metadataDoc)
        |> ignore

        s3GetObjectFacadeFactory <- Substitute.For<S3GetObjectFacadeFactory>()
        SetPrivateField(samlProvider, "s3GetObjectFacadeFactory", s3GetObjectFacadeFactory)

        s3GetObjectFacadeFactory.Create().Returns(s3GetObjectFacade)
        |> ignore

    [<SetUp>]
    let SetupIAM () =
        iamClient <- Substitute.For<IAmazonIdentityManagementService>()

        let response =
            UpdateSAMLProviderResponse(SAMLProviderArn = providerArn)

        iamClient.UpdateSAMLProviderAsync(Arg.Any<UpdateSAMLProviderRequest>()).Returns(response)
        |> ignore

        iamFactory <- Substitute.For<IamFactory>()
        SetPrivateField(samlProvider, "iamFactory", iamFactory)

        iamFactory.Create(Arg.Any<string>()).Returns(iamClient)
        |> ignore

    [<Test>]
    let ``An iam client is created with the requested role arn`` () =
        toTask
        <| async {
            let iamRole = "iamRoleArn"

            samlProvider.Request <- new Request<SamlProvider.Properties>()
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties(CreatorRoleArn = iamRole)
            samlProvider.Update() |> Async.AwaitTask |> ignore

            iamFactory.Received().Create(Arg.Is<string>(iamRole))
            |> ignore
            ()
           }

    [<Test>]
    let ``An s3GetObjectFacade is created`` () =
        toTask
        <| async {
            samlProvider.Request <- new Request<SamlProvider.Properties>()
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties()
            samlProvider.Update() |> Async.AwaitTask |> ignore

            s3GetObjectFacadeFactory.Received().Create()
            |> ignore
           }

    [<Test>]
    let ``The metadata doc is retrieved`` () =
        toTask
        <| async {
            let location = "location"
            let downloaderArn = "downloaderArn"

            samlProvider.Request <- new Request<SamlProvider.Properties>()
            samlProvider.Request.ResourceProperties <-
                SamlProvider.Properties(SamlMetadataDocumentLocation = location, DownloaderRoleArn = downloaderArn)
            samlProvider.Update() |> Async.AwaitTask |> ignore

            s3GetObjectFacade.Received().GetObject(Arg.Is(location), Arg.Is(downloaderArn))
            |> ignore
           }

    [<Test>]
    let ``The iam saml provider is updated`` () =
        toTask
        <| async {
            let providerArn = "arn"
            samlProvider.Request <- new Request<SamlProvider.Properties>(PhysicalResourceId = providerArn)
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties()
            samlProvider.Update() |> Async.AwaitTask |> ignore

            let request =
                fun (req: UpdateSAMLProviderRequest) ->
                    req.SAMLMetadataDocument = metadataDoc
                    && req.SAMLProviderArn = providerArn

            iamClient.Received().UpdateSAMLProviderAsync(Arg.Is<UpdateSAMLProviderRequest>(request))
            |> ignore
           }

    [<Test>]
    let ``The provider arn is returned in the response`` () =
        toTask
        <| async {
            let providerArn = "arn"
            samlProvider.Request <- new Request<SamlProvider.Properties>(PhysicalResourceId = providerArn)
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties()

            let! response = samlProvider.Update() |> Async.AwaitTask
            response.PhysicalResourceId
            |> should equal providerArn
           }

module Delete =
    let mutable samlProvider: SamlProvider = null
    let mutable iamFactory: IamFactory = null
    let mutable iamClient: IAmazonIdentityManagementService = null

    let metadataDoc = "doc"
    let providerArn = "providerArn"

    [<SetUp>]
    let SetUp = samlProvider <- SamlProvider()

    [<SetUp>]
    let SetupIAM () =
        iamClient <- Substitute.For<IAmazonIdentityManagementService>()

        let response =
            UpdateSAMLProviderResponse(SAMLProviderArn = providerArn)

        iamClient.UpdateSAMLProviderAsync(Arg.Any<UpdateSAMLProviderRequest>()).Returns(response)
        |> ignore

        iamFactory <- Substitute.For<IamFactory>()
        SetPrivateField(samlProvider, "iamFactory", iamFactory)

        iamFactory.Create(Arg.Any<string>()).Returns(iamClient)
        |> ignore

    [<Test>]
    let ``An iam client is created with the requested role arn`` () =
        toTask
        <| async {
            let iamRole = "iamRoleArn"

            samlProvider.Request <- new Request<SamlProvider.Properties>()
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties(CreatorRoleArn = iamRole)
            samlProvider.Delete() |> Async.AwaitTask |> ignore

            iamFactory.Received().Create(Arg.Is<string>(iamRole))
            |> ignore
            ()
           }

    [<Test>]
    let ``The saml provider is deleted`` () =
        toTask
        <| async {
            let providerArn = "arn"

            samlProvider.Request <- new Request<SamlProvider.Properties>(PhysicalResourceId = providerArn)
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties()
            samlProvider.Delete() |> Async.AwaitTask |> ignore

            let request =
                fun (req: DeleteSAMLProviderRequest) -> req.SAMLProviderArn = providerArn

            iamClient.Received().DeleteSAMLProviderAsync(Arg.Is<DeleteSAMLProviderRequest>(request))
            |> ignore
           }

    [<Test>]
    let ``The provider arn is returned in the response`` () =
        toTask
        <| async {
            let providerArn = "arn"
            samlProvider.Request <- new Request<SamlProvider.Properties>(PhysicalResourceId = providerArn)
            samlProvider.Request.ResourceProperties <- SamlProvider.Properties()

            let! response = samlProvider.Delete() |> Async.AwaitTask
            response.PhysicalResourceId
            |> should equal providerArn
           }
