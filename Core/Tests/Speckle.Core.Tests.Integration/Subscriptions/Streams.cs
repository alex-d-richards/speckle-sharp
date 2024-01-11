using Speckle.Core.Api;
using Speckle.Core.Api.SubscriptionModels;
using Speckle.Core.Credentials;

namespace Speckle.Core.Tests.Integration.Subscriptions;

public class Streams
{
  public Client client;

  private StreamInfo StreamAddedInfo;
  private string streamId;
  private StreamInfo StreamRemovedInfo;
  private StreamInfo StreamUpdatedInfo;
  public Account testUserAccount;

  [OneTimeSetUp]
  public async Task Setup()
  {
    testUserAccount = await Fixtures.SeedUser().ConfigureAwait(false);
    client = new Client(testUserAccount);
  }

  [Test, Order(0)]
  public async Task SubscribeStreamAdded()
  {
    client.SubscribeUserStreamAdded();
    client.OnUserStreamAdded += Client_OnUserStreamAdded;

    Thread.Sleep(1000); //let server catch-up

    var streamInput = new StreamCreateInput { description = "Hello World", name = "Super Stream 01" };

    var res = await client.StreamCreate(streamInput).ConfigureAwait(true);
    streamId = res;
    Assert.NotNull(res);

    await Task.Run(() =>
      {
        Thread.Sleep(1000); //let client catch-up
        Assert.NotNull(StreamAddedInfo);
        Assert.That(StreamAddedInfo.name, Is.EqualTo(streamInput.name));
      })
      .ConfigureAwait(false);
  }

  private void Client_OnUserStreamAdded(object sender, StreamInfo e)
  {
    StreamAddedInfo = e;
  }

  [Test, Order(1)]
  public async Task SubscribeStreamUpdated()
  {
    client.SubscribeStreamUpdated(streamId);
    client.OnStreamUpdated += Client_OnStreamUpdated;
    ;

    Thread.Sleep(100); //let server catch-up

    var streamInput = new StreamUpdateInput
    {
      id = streamId,
      description = "Hello World",
      name = "Super Stream 01 EDITED"
    };

    var res = await client.StreamUpdate(streamInput).ConfigureAwait(false);

    Assert.True(res);

    await Task.Run(() =>
      {
        Thread.Sleep(100); //let client catch-up
        Assert.NotNull(StreamUpdatedInfo);
        Assert.That(StreamUpdatedInfo.name, Is.EqualTo(streamInput.name));
      })
      .ConfigureAwait(false);
  }

  private void Client_OnStreamUpdated(object sender, StreamInfo e)
  {
    StreamUpdatedInfo = e;
  }

  [Test, Order(2)]
  public async Task SubscribeUserStreamRemoved()
  {
    client.SubscribeUserStreamRemoved();
    client.OnUserStreamRemoved += Client_OnStreamRemoved;
    ;

    Thread.Sleep(100); //let server catch-up

    var res = await client.StreamDelete(streamId).ConfigureAwait(false);

    Assert.True(res);

    await Task.Run(() =>
      {
        Thread.Sleep(100); //let client catch-up
        Assert.NotNull(StreamRemovedInfo);
        Assert.That(StreamRemovedInfo.id, Is.EqualTo(streamId));
      })
      .ConfigureAwait(false);
  }

  private void Client_OnStreamRemoved(object sender, StreamInfo e)
  {
    StreamRemovedInfo = e;
  }
}