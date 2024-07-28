using AspireApp.Tests.Helpers;
using System.Net;

namespace AspireApp.Tests;

public class WebTests
{
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireApp_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("webfrontend");
        var response = await httpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

	[Fact]
	public async Task GetWeatherReturnsRightContent()
	{
		// Arrange
		var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireApp_AppHost>();
		await using var app = await appHost.BuildAsync();
		await app.StartAsync();

		// Act
		var httpClient = app.CreateHttpClient("webfrontend");

		var response = await httpClient.GetAsync("/weather");
		var responseBody = await HtmlHelpers.GetDocumentAsync(response);
		var descriptionElement = responseBody.QuerySelector("p");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.NotNull(descriptionElement);
		Assert.Equal(
			"This component demonstrates showing data loaded from a backend API service.",
			descriptionElement.InnerHtml);
	}
}
