using System.ComponentModel.DataAnnotations;

namespace OpenIdConnectProvider.ViewModels.Authorization;

public class AuthorizeViewModel
{
	[Display(Name = "Application")]
	public string ApplicationName { get; set; }

	[Display(Name = "Scope")]
	public string Scope { get; set; }
}
