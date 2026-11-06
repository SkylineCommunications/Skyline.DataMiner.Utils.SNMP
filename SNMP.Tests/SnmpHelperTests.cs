namespace Skyline.DataMiner.Utils.SNMP.Tests
{
	using System;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Moq;

	using Skyline.DataMiner.Scripting;
	using Skyline.DataMiner.Utils.SNMP;

	[TestClass]
	public class SnmpHelperTests
	{
		private const int GroupId = 100;

		[TestMethod]
		public void IsSnmpAgentRestarted_False()
		{
			// Arrange
			SnmpDeltaHelper snmpDeltaHelper;
			SnmpHelper snmpHelper;
			string serialized = null;

			var protocolMock = new Mock<SLProtocol>();
			protocolMock
				.Setup(p => p.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, GroupId, null))
				.Returns(10000);    // 10.000 ms -> 10s

			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted(20);                            // 20s
			serialized = snmpHelper.ToJsonString();

			// Act
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			bool isAgentRestarted = snmpHelper.IsSnmpAgentRestarted(30);    // 20s + 10s = 30s

			// Assert
			Assert.IsFalse(isAgentRestarted);
		}

		[TestMethod]
		public void IsSnmpAgentRestarted_False_WithBufferedDelta()
		{
			// Arrange
			SnmpDeltaHelper snmpDeltaHelper;
			SnmpHelper snmpHelper;
			string serialized = null;

			var protocolMock = new Mock<SLProtocol>();
			protocolMock
				.Setup(p => p.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, GroupId, null))
				.Returns(10000);    // 10.000 ms -> 10s

			// Add First UpTime
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted(20);                            // 20s
			serialized = snmpHelper.ToJsonString();

			// Buffer delta (timeout)
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.BufferDelta();                                       // 20 + 10 = 30s
			serialized = snmpHelper.ToJsonString();

			// Act
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			bool isAgentRestarted = snmpHelper.IsSnmpAgentRestarted(40);    // 30 + 10 = 40s

			// Assert
			Assert.IsFalse(isAgentRestarted);
		}

		[TestMethod]
		public void IsSnmpAgentRestarted_False_WithBufferedDeltaReset()
		{
			// Arrange
			SnmpDeltaHelper snmpDeltaHelper;
			SnmpHelper snmpHelper;
			string serialized = null;

			var protocolMock = new Mock<SLProtocol>();
			protocolMock
				.Setup(p => p.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, GroupId, null))
				.Returns(10000);    // 10.000 ms -> 10s

			// Add First UpTime
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted(20);                            // 20s
			serialized = snmpHelper.ToJsonString();

			// Buffer delta (timeout)
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.BufferDelta();                                       // 20 + 10 = 30s
			serialized = snmpHelper.ToJsonString();

			// Buffered delta reset
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted(20);                            // 0 + 20s = 20s
			serialized = snmpHelper.ToJsonString();

			// Act
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			bool isAgentRestarted = snmpHelper.IsSnmpAgentRestarted(30);    // 20 + 10 = 30

			// Assert
			Assert.IsFalse(isAgentRestarted);
		}

		[TestMethod]
		public void IsSnmpAgentRestarted_FalseWithWraparound()
		{
			// Arrange
			SnmpDeltaHelper snmpDeltaHelper;
			SnmpHelper snmpHelper;
			string serialized = null;

			var protocolMock = new Mock<SLProtocol>();
			protocolMock
				.Setup(p => p.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, GroupId, null))
				.Returns(10000);    // 10.000 ms -> 10s

			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted((UInt32.MaxValue / 100) - 5.0);     // max seconds - 5s
			serialized = snmpHelper.ToJsonString();

			// Act
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			bool isAgentRestarted = snmpHelper.IsSnmpAgentRestarted(4);         // (max seconds - 5s) + 10s = 4s -> wraparound -> no restart

			// Assert
			Assert.IsFalse(isAgentRestarted);
		}

		[TestMethod]
		public void IsSnmpAgentRestarted_FalseWithWraparound_WithBufferedDelta()
		{
			// Arrange
			SnmpDeltaHelper snmpDeltaHelper;
			SnmpHelper snmpHelper;
			string serialized = null;

			var protocolMock = new Mock<SLProtocol>();
			protocolMock
				.Setup(p => p.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, GroupId, null))
				.Returns(10000);    // 10.000 ms -> 10s

			// Add First UpTime
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted((UInt32.MaxValue / 100) - 15.0);    // max seconds - 15s
			serialized = snmpHelper.ToJsonString();

			// Buffer delta (timeout)
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.BufferDelta();                                           // (max seconds - 15s) + 10s = max seconds - 5s
			serialized = snmpHelper.ToJsonString();

			// Act
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			bool isAgentRestarted = snmpHelper.IsSnmpAgentRestarted(4);         // (max seconds - 5s) + 10s = 4s -> wraparound -> no restart

			// Assert
			Assert.IsFalse(isAgentRestarted);
		}

		[TestMethod]
		public void IsSnmpAgentRestarted_FalseWithWraparound_WithBufferedDeltaReset()
		{
			// Arrange
			SnmpDeltaHelper snmpDeltaHelper;
			SnmpHelper snmpHelper;
			string serialized = null;

			var protocolMock = new Mock<SLProtocol>();
			protocolMock
				.Setup(p => p.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, GroupId, null))
				.Returns(10000);    // 10.000 ms -> 10s

			// Add First UpTime
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted((UInt32.MaxValue / 100) - 5.0);    // max seconds - 5s
			serialized = snmpHelper.ToJsonString();

			// Buffer delta (timeout)
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.BufferDelta();                                           // (max seconds - 5) + 10s = max seconds + 5s
			serialized = snmpHelper.ToJsonString();

			// Buffered delta reset
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted((UInt32.MaxValue / 100) - 5.0);    // max seconds - 5s
			serialized = snmpHelper.ToJsonString();

			// Act
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			bool isAgentRestarted = snmpHelper.IsSnmpAgentRestarted(4);         // (max seconds - 5s) + 10s = 4s -> wraparound -> no restart

			// Assert
			Assert.IsFalse(isAgentRestarted);
		}

		[TestMethod]
		public void IsSnmpAgentRestarted_True()
		{
			// Arrange
			SnmpDeltaHelper snmpDeltaHelper;
			SnmpHelper snmpHelper;
			string serialized = null;

			var protocolMock = new Mock<SLProtocol>();
			protocolMock
				.Setup(p => p.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, GroupId, null))
				.Returns(10000);    // 10.000 ms -> 10s

			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted((UInt32.MaxValue / 100) - 15.0);    // max seconds - 15s
			serialized = snmpHelper.ToJsonString();

			// Act
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			bool isAgentRestarted = snmpHelper.IsSnmpAgentRestarted(5);        // (max seconds - 15s) + 10s = max seconds - 5s -> no wraparound yet -> restart

			// Assert
			Assert.IsTrue(isAgentRestarted);
		}

		[TestMethod]
		public void IsSnmpAgentRestarted_True_WithBufferedDelta()
		{
			// Arrange
			SnmpDeltaHelper snmpDeltaHelper;
			SnmpHelper snmpHelper;
			string serialized = null;

			var protocolMock = new Mock<SLProtocol>();
			protocolMock
				.Setup(p => p.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, GroupId, null))
				.Returns(10000);    // 10.000 ms -> 10s

			// Add First UpTime
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted((UInt32.MaxValue / 100) - 25.0);    // max seconds - 25s
			serialized = snmpHelper.ToJsonString();

			// Buffer delta (timeout)
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.BufferDelta();                                           // (max seconds - 25s) + 10s = max seconds - 15s
			serialized = snmpHelper.ToJsonString();

			// Act
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			bool isAgentRestarted = snmpHelper.IsSnmpAgentRestarted(5);         // (max seconds - 15s) + 10s = 	(max seconds - 5s) -> No wraparound yet -> restart

			// Assert
			Assert.IsTrue(isAgentRestarted);
		}

		[TestMethod]
		public void IsSnmpAgentRestarted_True_WithBufferedDeltaReset()
		{
			// Arrange
			SnmpDeltaHelper snmpDeltaHelper;
			SnmpHelper snmpHelper;
			string serialized = null;

			var protocolMock = new Mock<SLProtocol>();
			protocolMock
				.Setup(p => p.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, GroupId, null))
				.Returns(10000);    // 10.000 ms -> 10s

			// Add First UpTime
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted((UInt32.MaxValue / 100) - 15);      // max seconds - 15s
			serialized = snmpHelper.ToJsonString();

			// Buffer delta (timeout)
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.BufferDelta();                                           // (max seconds - 15s) + 10s = max seconds - 5s
			serialized = snmpHelper.ToJsonString();

			// Buffer delta reset
			/*
			 * Note that if the BufferedDelta was not properly reset, the wraparound would happen and this unit test would fail.
			 * */
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			snmpHelper.IsSnmpAgentRestarted((UInt32.MaxValue / 100) - 15.0);    // max seconds - 15s
			serialized = snmpHelper.ToJsonString();

			// Act
			snmpDeltaHelper = new SnmpDeltaHelper(protocolMock.Object, GroupId);
			snmpHelper = SnmpHelper.FromJsonString(serialized, snmpDeltaHelper);
			bool isAgentRestarted = snmpHelper.IsSnmpAgentRestarted(5);        // (max seconds - 15s) + 10s = max seconds - 5s -> no wraparound yet -> restart

			// Assert
			Assert.IsTrue(isAgentRestarted);
		}
	}
}
