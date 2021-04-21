using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace PhoneDetector
{
	class Program
	{
		public static int PingTimeout => 1000;
		public static int PingInterval => 1000;

		public static int PingsTimedOutForVerifyPhoneIsntConnected = 60;

		static int pingnotSuccess = 0;


		//curl -s -X POST -H "Content-Type: application/json" -d '{"parameters":{}}' http://192.168.1.1/sysbus/Devices:get | jq
		static async Task Main(string[] args)
		{
			Console.WriteLine("==================================================================");
			Console.WriteLine($"Estimated time to verify that device isnt connected: {PingInterval + ((PingTimeout + PingInterval) * PingsTimedOutForVerifyPhoneIsntConnected)}ms");
			Console.WriteLine("==================================================================");
			Console.WriteLine();

			if (File.Exists("ip.txt"))
			{
				string ipToCheck = File.ReadAllText("ip.txt");
				bool DeviceState = false;

				while (true)
				{
					Ping ping = new Ping();
					PingReply pingReply = ping.Send(ipToCheck, PingTimeout);
					if (args.Length == 1 && args[0] == "true") Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {pingReply.Status}");

					if (pingReply.Status == IPStatus.Success)
					{
						if (!DeviceState)
						{
							Console.WriteLine($"[{DateTime.Now}] DEVICE TURNED ON!");
							DeviceState = true;
							//new WebClient().DownloadString("http://192.168.1.18:21377/stt/powiedz%20Telefon%20jest%20połączony!");
							await $"bash con.sh".Bash();
						}
						pingnotSuccess = 0;
					}
					else if (pingReply.Status != IPStatus.Success && DeviceState)
					{
						pingnotSuccess++;
						if (pingnotSuccess == PingsTimedOutForVerifyPhoneIsntConnected)
						{
							Console.WriteLine($"[{DateTime.Now}] DEVICE TURNED OFF!");
							pingnotSuccess = 0;
							DeviceState = false;
							//new WebClient().DownloadString("http://192.168.1.18:21377/stt/powiedz%20Telefon%20nie%20jest%20połączony!");
							await $"bash uncon.sh".Bash();
						}
					}

					Thread.Sleep(PingInterval);
				}
			}
			else
			{
				Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] FILE ip.txt with device id doesn't exists!");
			}
		}

		static async Task<string> GetDevices()
		{
			using (var httpClient = new HttpClient())
			{
				using (var request = new HttpRequestMessage(new HttpMethod("POST"), "http://192.168.1.1/sysbus/Devices:get"))
				{
					request.Content = new StringContent("{\"parameters\":{}}");
					request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

					var response = await httpClient.SendAsync(request);
					return await response.Content.ReadAsStringAsync();
				}
			}
		}
	}

	public static class Extensions
	{
		public static Task<int> Bash(this string cmd)
		{
			var source = new TaskCompletionSource<int>();
			var escapedArgs = cmd.Replace("\"", "\\\"");
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "bash",
					Arguments = $"-c \"{escapedArgs}\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				},
				EnableRaisingEvents = true
			};
			process.Exited += (sender, args) =>
			{
				if (process.ExitCode == 0)
				{
					source.SetResult(0);
				}
				else
				{
					source.SetException(new Exception($"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
				}

				process.Dispose();
			};

			try
			{
				process.Start();
			}
			catch (Exception e)
			{
				source.SetException(e);
			}

			return source.Task;
		}
	}


	public class JsonDevices
	{
		public Result result { get; set; }
	}

	public class Result
	{
		public Status[] status { get; set; }
	}

	public class Status
	{
		public string Key { get; set; }
		public string DiscoverySource { get; set; }
		public string Name { get; set; }
		public string DeviceType { get; set; }
		public bool Active { get; set; }
		public string Tags { get; set; }
		public DateTime LastConnection { get; set; }
		public DateTime LastChanged { get; set; }
		public string Manufacturer { get; set; }
		public string ModelName { get; set; }
		public string Description { get; set; }
		public string SerialNumber { get; set; }
		public string ProductClass { get; set; }
		public string HardwareVersion { get; set; }
		public string SoftwareVersion { get; set; }
		public string BootLoaderVersion { get; set; }
		public string FirewallLevel { get; set; }
		public string LinkType { get; set; }
		public string LinkState { get; set; }
		public string ConnectionProtocol { get; set; }
		public string ConnectionState { get; set; }
		public string LastConnectionError { get; set; }
		public string ConnectionIPv4Address { get; set; }
		public string ConnectionIPv6Address { get; set; }
		public string RemoteGateway { get; set; }
		public string DNSServers { get; set; }
		public bool Internet { get; set; }
		public bool IPTV { get; set; }
		public bool Telephony { get; set; }
		public string Index { get; set; }
		public Name[] Names { get; set; }
		public Devicetype[] DeviceTypes { get; set; }
		public string PhysAddress { get; set; }
		public bool Ageing { get; set; }
		public string Layer2Interface { get; set; }
		public string IPAddress { get; set; }
		public string IPAddressSource { get; set; }
		public string VendorClassID { get; set; }
		public string UserClassID { get; set; }
		public string ClientID { get; set; }
		public string OUI { get; set; }
		public Action[] Actions { get; set; }
		public Ipv4address[] IPv4Address { get; set; }
		public Ipv6address[] IPv6Address { get; set; }
		public Mdnsservice[] mDNSService { get; set; }
		public string BusName { get; set; }
		public string NetDevName { get; set; }
		public int NetDevIndex { get; set; }
		public string DHCPv4ServerPool { get; set; }
		public bool DHCPv4ServerEnable { get; set; }
		public string DHCPv4ServerMinAddress { get; set; }
		public string DHCPv4ServerMaxAddress { get; set; }
		public string DHCPv4ServerNetmask { get; set; }
		public string DHCPv4DomainName { get; set; }
		public string SSID { get; set; }
		public string BSSID { get; set; }
		public string OperatingFrequencyBand { get; set; }
		public int MaxBitRateSupported { get; set; }
		public int CurrentBitRate { get; set; }
		public int Port { get; set; }
		public string USBHost { get; set; }
		public string DirectoryNumber { get; set; }
		public string EndpointType { get; set; }
		public string OutgoingTrunkLine { get; set; }
		public string Type { get; set; }
		public string ManufacturerURL { get; set; }
		public string ModelDescription { get; set; }
		public string ModelNumber { get; set; }
		public string ModelURL { get; set; }
		public string UDN { get; set; }
		public string UPC { get; set; }
		public string PresentationURL { get; set; }
		public string Server { get; set; }
		public Service[] Service { get; set; }
	}

	public class Name
	{
		public string name { get; set; }
		public string Source { get; set; }
	}

	public class Devicetype
	{
		public string Type { get; set; }
		public string Source { get; set; }
	}

	public class Action
	{
		public string Function { get; set; }
		public string Name { get; set; }
		public Argument[] Arguments { get; set; }
	}

	public class Argument
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public bool Mandatory { get; set; }
	}

	public class Ipv4address
	{
		public string Address { get; set; }
		public string Status { get; set; }
		public string Scope { get; set; }
		public string AddressSource { get; set; }
		public bool Reserved { get; set; }
	}

	public class Ipv6address
	{
		public string Address { get; set; }
		public string Status { get; set; }
		public string Scope { get; set; }
		public string AddressSource { get; set; }
	}

	public class Mdnsservice
	{
		public string Name { get; set; }
		public string ServiceName { get; set; }
		public string Domain { get; set; }
		public string Port { get; set; }
		public string Text { get; set; }
	}

	public class Service
	{
		public string ServiceType { get; set; }
		public string ServiceId { get; set; }
		public string SCPDURL { get; set; }
		public string ControlURL { get; set; }
		public string EventSubURL { get; set; }
	}

}
