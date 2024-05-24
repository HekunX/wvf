﻿using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace HSoft.WebView.NetFramework.WPF
{
    public  class StaticContentProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly Uri _appBaseUri;
        private readonly string _hostPageRelativePath;
        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

        static StaticContentProvider()
        {

        }
        public StaticContentProvider(IFileProvider fileProvider, Uri appBaseUri, string hostPageRelativePath)
        {

            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _appBaseUri = appBaseUri ?? throw new ArgumentNullException(nameof(appBaseUri));
            _hostPageRelativePath = hostPageRelativePath ?? throw new ArgumentNullException(nameof(hostPageRelativePath));
        }

        public bool TryGetResponseContent(string requestUri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out IDictionary<string, string> headers)
        {
            var fileUri = new Uri(requestUri);
            if (_appBaseUri.IsBaseOf(fileUri))
            {
                var relativePath = _appBaseUri.MakeRelativeUri(fileUri).ToString();

                // Content in the file provider takes first priority
                // Next we may fall back on supplying the host page to support deep linking
                // If there's no match, fall back on serving embedded framework content
                string contentType;
                var found = TryGetFromFileProvider(relativePath, out content, out contentType)
                    || (allowFallbackOnHostPage && TryGetFromFileProvider(_hostPageRelativePath, out content, out contentType))
                    || TryGetFrameworkFile(relativePath, out content, out contentType);

                if (found)
                {
                    statusCode = 200;
                    statusMessage = "OK";
                    headers = GetResponseHeaders(contentType);
                }
                else
                {
                    content = new MemoryStream(Encoding.UTF8.GetBytes($"There is no content at {relativePath}"));
                    statusCode = 404;
                    statusMessage = "Not found";
                    headers = GetResponseHeaders("text/plain");
                }

                // Always respond to requests within the base URI, even if there's no matching file
                return true;
            }
            else
            {
                // URL isn't within application base path, so let the network handle it
                statusCode = default;
                statusMessage = default;
                headers = default;
                content = default;
                return false;
            }
        }

        public bool TryGetFromFileProvider(string relativePath, out Stream content, out string contentType)
        {
            if (!string.IsNullOrEmpty(relativePath))
            {
                var fileInfo = _fileProvider.GetFileInfo(relativePath);
                if (fileInfo.Exists)
                {
                    content = fileInfo.CreateReadStream();
                    contentType = GetResponseContentTypeOrDefault(fileInfo.Name);
                    return true;
                }
            }

            content = default;
            contentType = default;
            return false;
        }


        public static bool TryGetFrameworkFile(string relativePath, out Stream content, out string contentType)
        {
            // 获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 获取嵌入的资源名称
            string[] resourceNames = assembly.GetManifestResourceNames();

            var stream = assembly.GetManifestResourceStream(resourceNames[0]);


            // We're not trying to simulate everything a real webserver does. We don't need to
            // support querystring parameters, for example. It's enough to require an exact match.
          
            if (stream != null)
            {
                content = stream;
                contentType = GetResponseContentTypeOrDefault(relativePath);
                return true;
            }

            content = default;
            contentType = default;
            return false;
        }

        private static string GetResponseContentTypeOrDefault(string path)
            => ContentTypeProvider.TryGetContentType(path, out var matchedContentType)
            ? matchedContentType
            : "application/octet-stream";

        private static IDictionary<string, string> GetResponseHeaders(string contentType)
            => new Dictionary<string, string>()
            {
                { "Content-Type", contentType },
                { "Cache-Control", "no-cache, max-age=0, must-revalidate, no-store" },
            };
    }
}
