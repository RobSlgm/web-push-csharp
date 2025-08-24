﻿using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebPush.Test;

[TestClass]
public class VapidHelperTest
{
    private const string ValidAudience = @"http://example.com";
    private const string ValidSubject = @"http://example.com/example";
    private const string ValidSubjectMailto = @"mailto:example@example.com";

    private const string TestPublicKey =
        @"BCvKwB2lbVUYMFAaBUygooKheqcEU-GDrVRnu8k33yJCZkNBNqjZj0VdxQ2QIZa4kV5kpX9aAqyBKZHURm6eG1A";

    private const string TestPrivateKey = @"on6X5KmLEFIVvPP3cNX9kE0OF6PV9TJQXVbnKU2xEHI";

    [TestMethod]
    public void TestGenerateVapidKeys()
    {
        var keys = VapidHelper.GenerateVapidKeys();
        var publicKey = Base64UrlEncoder.DecodeBytes(keys.PublicKey);
        var privateKey = Base64UrlEncoder.DecodeBytes(keys.PrivateKey);

        Assert.AreEqual(32, privateKey.Length);
        Assert.AreEqual(65, publicKey.Length);
    }

    [TestMethod]
    public void TestGenerateVapidKeysNoCache()
    {
        var keys1 = VapidHelper.GenerateVapidKeys();
        var keys2 = VapidHelper.GenerateVapidKeys();

        Assert.AreNotEqual(keys1.PublicKey, keys2.PublicKey);
        Assert.AreNotEqual(keys1.PrivateKey, keys2.PrivateKey);
    }

    [TestMethod]
    public void TestGetVapidHeaders()
    {
        var publicKey = TestPublicKey;
        var privateKey = TestPrivateKey;
        var headers = VapidHelper.GetVapidHeaders(ValidAudience, ValidSubject, publicKey, privateKey);

        Assert.IsTrue(headers.ContainsKey(@"Authorization"));
        // Assert.IsTrue(headers.ContainsKey(@"Crypto-Key"));
    }

    [TestMethod]
    public void TestGetVapidHeadersAudienceNotAUrl()
    {
        var publicKey = TestPublicKey;
        var privateKey = TestPrivateKey;
        Assert.ThrowsExactly<ArgumentException>(
            delegate
            {
                VapidHelper.GetVapidHeaders("invalid audience", ValidSubjectMailto, publicKey, privateKey);
            });
    }

    [TestMethod]
    public void TestGetVapidHeadersInvalidPrivateKey()
    {
        var publicKey = Base64UrlEncoder.Encode(new byte[65]);
        var privateKey = Base64UrlEncoder.Encode(new byte[1]);

        Assert.ThrowsExactly<ArgumentException>(
            delegate { VapidHelper.GetVapidHeaders(ValidAudience, ValidSubject, publicKey, privateKey); });
    }

    [TestMethod]
    public void TestGetVapidHeadersInvalidPublicKey()
    {
        var publicKey = Base64UrlEncoder.Encode(new byte[1]);
        var privateKey = Base64UrlEncoder.Encode(new byte[32]);

        Assert.ThrowsExactly<ArgumentException>(
            delegate { VapidHelper.GetVapidHeaders(ValidAudience, ValidSubject, publicKey, privateKey); });
    }

    [TestMethod]
    public void TestGetVapidHeadersSubjectNotAUrlOrMailTo()
    {
        var publicKey = TestPublicKey;
        var privateKey = TestPrivateKey;

        Assert.ThrowsExactly<ArgumentException>(
            delegate { VapidHelper.GetVapidHeaders(ValidAudience, @"invalid subject", publicKey, privateKey); });
    }

    [TestMethod]
    public void TestGetVapidHeadersWithMailToSubject()
    {
        var publicKey = TestPublicKey;
        var privateKey = TestPrivateKey;
        var headers = VapidHelper.GetVapidHeaders(ValidAudience, ValidSubjectMailto, publicKey,
            privateKey);

        Assert.IsTrue(headers.ContainsKey(@"Authorization"));
        // Assert.IsTrue(headers.ContainsKey(@"Crypto-Key"));
    }

    [TestMethod]
    public void TestExpirationInPastExceptions()
    {
        var publicKey = TestPublicKey;
        var privateKey = TestPrivateKey;

        Assert.ThrowsExactly<ArgumentException>(
            delegate
            {
                VapidHelper.GetVapidHeaders(ValidAudience, ValidSubjectMailto, publicKey,
                    privateKey, DateTimeOffset.FromUnixTimeSeconds(1552715607).UtcDateTime);
            });
    }
}