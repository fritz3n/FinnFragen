using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FinnFragen.Web.Services
{
	public class CaptchaValidator
	{
		private readonly IHttpContextAccessor contextAccessor;
		private readonly IConfiguration configuration;
		private readonly IHttpClientFactory clientFactory;
		private readonly string secret;

		public CaptchaValidator(IHttpContextAccessor contextAccessor, IConfiguration configuration, IHttpClientFactory clientFactory)
		{
			this.contextAccessor = contextAccessor;
			this.configuration = configuration;
			this.clientFactory = clientFactory;
			secret = configuration.GetValue<string>("Captcha:SecretKey");
		}

		public async Task<bool> Validate()
		{
			HttpContext context = contextAccessor.HttpContext;

			string clientResponse = context.Request.Form["g-Recaptcha-Response"].ToString();
			if (clientResponse == string.Empty)
				return false;

			string remote;

			if (context.Request.Headers.Keys.Contains("X-Proxy-For"))
			{
				remote = context.Request.Headers["X-Proxy-For"];
			}
			else
			{
				remote = context.Connection.RemoteIpAddress.ToString();
			}


			HttpClient client = clientFactory.CreateClient("recaptcha");

			var values = new Dictionary<string, string>
			{
				{ "secret", secret },
				{ "response", clientResponse },
				{ "remoteip", remote}
			};

			var content = new FormUrlEncodedContent(values);

			HttpResponseMessage response = await client.PostAsync("/recaptcha/api/siteverify", content);
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
			};
			string json = await response.Content.ReadAsStringAsync();
			CaptchaResponse captchaResponse = JsonSerializer.Deserialize<CaptchaResponse>(json, options);

			return captchaResponse.Success;
		}

		class CaptchaResponse
		{
			public bool Success { get; set; }
			[JsonPropertyName("error-codes")]
			public string[] ErrorCodes { get; set; }
		}
	}
}
