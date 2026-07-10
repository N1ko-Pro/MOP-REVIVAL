// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Минимальный HTTPS-GET клиент на управляемом TLS от BouncyCastle: старый Mono игры не умеет TLS 1.2,
// поэтому поверх сырого TCP-сокета гоняется TLS-клиент BouncyCastle (с SNI), а HTTP/1.1 разбирается вручную.

using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;

namespace MOPR.Rules
{
    internal static class TlsHttpClient
    {
        /// <summary>HTTPS GET. Возвращает тело ответа или null с заполненным <paramref name="error"/>.</summary>
        public static string Get(string url, int timeoutMs, out string error)
        {
            error = null;
            TcpClient tcp = null;

            try
            {
                ParseUrl(url, out string host, out int port, out string path);

                tcp = new TcpClient();
                if (!ConnectWithTimeout(tcp, host, port, timeoutMs))
                {
                    error = "connect timeout";
                    return null;
                }

                tcp.ReceiveTimeout = timeoutMs;
                tcp.SendTimeout = timeoutMs;

                TlsClientProtocol protocol = new TlsClientProtocol(tcp.GetStream(), new SecureRandom());
                protocol.Connect(new SniTlsClient(host));
                Stream tls = protocol.Stream;

                string request =
                    "GET " + path + " HTTP/1.1\r\n" +
                    "Host: " + host + "\r\n" +
                    "User-Agent: MOPR/" + MOPR.ModVersion + "\r\n" +
                    "Accept: */*\r\n" +
                    "Connection: close\r\n\r\n";
                byte[] requestBytes = Encoding.ASCII.GetBytes(request);
                tls.Write(requestBytes, 0, requestBytes.Length);
                tls.Flush();

                byte[] raw = ReadToEnd(tls);
                try { protocol.Close(); } catch { }

                return ParseHttpResponse(raw, out error);
            }
            catch (Exception ex)
            {
                error = ex.GetType().Name + ": " + ex.Message;
                return null;
            }
            finally
            {
                try { if (tcp != null) tcp.Close(); } catch { }
            }
        }

        private static void ParseUrl(string url, out string host, out int port, out string path)
        {
            string rest = url;
            if (rest.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                rest = rest.Substring("https://".Length);
            }
            else if (rest.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                rest = rest.Substring("http://".Length);
            }

            int slashIndex = rest.IndexOf('/');
            string authority = slashIndex >= 0 ? rest.Substring(0, slashIndex) : rest;
            path = slashIndex >= 0 ? rest.Substring(slashIndex) : "/";

            int colonIndex = authority.IndexOf(':');
            if (colonIndex >= 0)
            {
                host = authority.Substring(0, colonIndex);
                int.TryParse(authority.Substring(colonIndex + 1), out port);
                if (port <= 0)
                {
                    port = 443;
                }
            }
            else
            {
                host = authority;
                port = 443;
            }
        }

        private static bool ConnectWithTimeout(TcpClient tcp, string host, int port, int timeoutMs)
        {
            IAsyncResult ar = tcp.BeginConnect(host, port, null, null);
            if (!ar.AsyncWaitHandle.WaitOne(timeoutMs))
            {
                return false;
            }

            tcp.EndConnect(ar);
            return true;
        }

        private static byte[] ReadToEnd(Stream stream)
        {
            MemoryStream buffer = new MemoryStream();
            byte[] chunk = new byte[8192];

            try
            {
                int read;
                while ((read = stream.Read(chunk, 0, chunk.Length)) > 0)
                {
                    buffer.Write(chunk, 0, read);
                }
            }
            catch
            {
                // Таймаут сокета или reset после отправки всего: используем, что успели прочитать.
            }

            return buffer.ToArray();
        }

        private static string ParseHttpResponse(byte[] raw, out string error)
        {
            error = null;

            int headerEnd = IndexOfDoubleCrlf(raw);
            if (headerEnd < 0)
            {
                error = "malformed http response";
                return null;
            }

            string headerText = Encoding.ASCII.GetString(raw, 0, headerEnd);
            string[] lines = headerText.Split(new[] { "\r\n" }, StringSplitOptions.None);

            int statusCode = ParseStatusCode(lines.Length > 0 ? lines[0] : string.Empty);
            if (statusCode != 200)
            {
                error = "http " + statusCode;
                return null;
            }

            bool chunked = false;
            int contentLength = -1;
            for (int i = 1; i < lines.Length; i++)
            {
                int colonIndex = lines[i].IndexOf(':');
                if (colonIndex < 0)
                {
                    continue;
                }

                string key = lines[i].Substring(0, colonIndex).Trim().ToLower();
                string value = lines[i].Substring(colonIndex + 1).Trim();

                if (key == "transfer-encoding" && value.ToLower().Contains("chunked"))
                {
                    chunked = true;
                }
                else if (key == "content-length")
                {
                    int.TryParse(value, out contentLength);
                }
            }

            int bodyStart = headerEnd + 4;
            byte[] body;

            if (chunked)
            {
                body = DeChunk(raw, bodyStart);
            }
            else
            {
                int available = raw.Length - bodyStart;
                int length = contentLength >= 0 ? Math.Min(contentLength, available) : available;
                body = new byte[length];
                Array.Copy(raw, bodyStart, body, 0, length);
            }

            return Encoding.UTF8.GetString(body);
        }

        private static byte[] DeChunk(byte[] raw, int pos)
        {
            MemoryStream output = new MemoryStream();

            while (pos < raw.Length)
            {
                int lineEnd = IndexOf(raw, (byte)'\r', pos);
                if (lineEnd < 0)
                {
                    break;
                }

                string sizeLine = Encoding.ASCII.GetString(raw, pos, lineEnd - pos).Trim();
                int semicolonIndex = sizeLine.IndexOf(';');
                if (semicolonIndex >= 0)
                {
                    sizeLine = sizeLine.Substring(0, semicolonIndex);
                }

                if (!TryParseHex(sizeLine, out int size))
                {
                    break;
                }

                pos = lineEnd + 2; // пропускаем CRLF после строки размера
                if (size <= 0)
                {
                    break;
                }

                if (pos + size > raw.Length)
                {
                    size = raw.Length - pos;
                }

                output.Write(raw, pos, size);
                pos += size + 2; // пропускаем данные чанка и завершающий CRLF
            }

            return output.ToArray();
        }

        private static int IndexOfDoubleCrlf(byte[] data)
        {
            for (int i = 0; i + 3 < data.Length; i++)
            {
                if (data[i] == '\r' && data[i + 1] == '\n' && data[i + 2] == '\r' && data[i + 3] == '\n')
                {
                    return i;
                }
            }

            return -1;
        }

        private static int IndexOf(byte[] data, byte value, int start)
        {
            for (int i = start; i < data.Length; i++)
            {
                if (data[i] == value)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int ParseStatusCode(string statusLine)
        {
            // "HTTP/1.1 200 OK"
            string[] parts = statusLine.Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int code))
            {
                return code;
            }

            return -1;
        }

        private static bool TryParseHex(string text, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            try
            {
                value = Convert.ToInt32(text.Trim(), 16);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>TLS-клиент BouncyCastle: принимает любой сертификат сервера и шлёт SNI.</summary>
        private sealed class SniTlsClient : DefaultTlsClient
        {
            private readonly string host;

            public SniTlsClient(string host)
            {
                this.host = host;
            }

            public override TlsAuthentication GetAuthentication()
            {
                return new AcceptAllAuthentication();
            }

            public override IDictionary GetClientExtensions()
            {
                IDictionary extensions = base.GetClientExtensions();
                if (extensions == null)
                {
                    extensions = new Hashtable();
                }

                ArrayList names = new ArrayList { new ServerName(NameType.host_name, host) };
                TlsExtensionsUtilities.AddServerNameExtension(extensions, new ServerNameList(names));
                return extensions;
            }
        }

        private sealed class AcceptAllAuthentication : TlsAuthentication
        {
            public void NotifyServerCertificate(Certificate serverCertificate)
            {
                // Публичные несекретные данные; корневого хранилища у рантайма всё равно нет.
            }

            public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
            {
                return null; // без клиентского сертификата (нет mutual TLS)
            }
        }
    }
}
