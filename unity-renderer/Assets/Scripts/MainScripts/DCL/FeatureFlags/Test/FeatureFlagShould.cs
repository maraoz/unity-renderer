using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FeatureFlagShould
{
    [SetUp]
    public void SetUp() { }

    [TearDown]
    public void TearDown() { }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void AddFeaturesCorrectly(bool isEnabled)
    {
        //Arrange
        FeatureFlag featureFlag = new FeatureFlag();

        //Act
        featureFlag.flags.Add("Test", isEnabled);

        //Assert
        Assert.AreEqual(isEnabled, featureFlag.IsFeatureEnabled("Test"));
    }

    [Test]
    public void AddBridgeWhenInitialized()
    {
        //Arrange
        GameObject bridgeGameObject = new GameObject("Bridges");
        FeatureFlagController controller = new FeatureFlagController(bridgeGameObject);
        GameObject newGameObject = new GameObject("Test");

        //Act
        controller.AddBridgeComponent(newGameObject);

        //Assert
        Assert.IsNotNull(controller.featureFlagBridgeComponent);
        Assert.IsNotNull(newGameObject.GetComponent<FeatureFlagBridge>());
        Object.Destroy(newGameObject);
        Object.Destroy(bridgeGameObject);
    }

    [Test]
    public void BeDisposedCorrectly()
    {
        //Arrange
        FeatureFlagController controller = new FeatureFlagController();

        //Act
        controller.Dispose();

        //Assert
        Assert.IsNull(controller.featureFlagBridgeComponent);
    }

    [Test]
    public void ApplyFeatureFlagWhenBridgeReceivesUpdateMessage()
    {
        //Arrange
        GameObject newGameObject = new GameObject("Test");
        FeatureFlagBridge bridge = newGameObject.AddComponent<FeatureFlagBridge>();
        FeatureFlag config = TestHelpers.CreateFeatureFlag();
        DataStore.i.featureFlags.flags.OnChange += FlagConfigReceived;
        var featureFlagReceived = false;

        void FlagConfigReceived(FeatureFlag newConfig, FeatureFlag oldConfig) => featureFlagReceived = true;

        //Act
        bridge.SetFeatureFlagConfiguration(config);

        //Assert
        Assert.IsTrue(featureFlagReceived);
    }
}