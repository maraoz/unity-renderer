// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: AudioStream.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from AudioStream.proto</summary>
  public static partial class AudioStreamReflection {

    #region Descriptor
    /// <summary>File descriptor for AudioStream.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static AudioStreamReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChFBdWRpb1N0cmVhbS5wcm90bxIQZGVjZW50cmFsYW5kLmVjcyI9Cg1QQkF1",
            "ZGlvU3RyZWFtEg8KB3BsYXlpbmcYASABKAgSDgoGdm9sdW1lGAIgASgCEgsK",
            "A3VybBgDIAEoCUIUqgIRRENMLkVDU0NvbXBvbmVudHNiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBAudioStream), global::DCL.ECSComponents.PBAudioStream.Parser, new[]{ "Playing", "Volume", "Url" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBAudioStream : pb::IMessage<PBAudioStream> {
    private static readonly pb::MessageParser<PBAudioStream> _parser = new pb::MessageParser<PBAudioStream>(() => new PBAudioStream());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<PBAudioStream> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.AudioStreamReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PBAudioStream() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PBAudioStream(PBAudioStream other) : this() {
      playing_ = other.playing_;
      volume_ = other.volume_;
      url_ = other.url_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PBAudioStream Clone() {
      return new PBAudioStream(this);
    }

    /// <summary>Field number for the "playing" field.</summary>
    public const int PlayingFieldNumber = 1;
    private bool playing_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Playing {
      get { return playing_; }
      set {
        playing_ = value;
      }
    }

    /// <summary>Field number for the "volume" field.</summary>
    public const int VolumeFieldNumber = 2;
    private float volume_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Volume {
      get { return volume_; }
      set {
        volume_ = value;
      }
    }

    /// <summary>Field number for the "url" field.</summary>
    public const int UrlFieldNumber = 3;
    private string url_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Url {
      get { return url_; }
      set {
        url_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as PBAudioStream);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(PBAudioStream other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Playing != other.Playing) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Volume, other.Volume)) return false;
      if (Url != other.Url) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Playing != false) hash ^= Playing.GetHashCode();
      if (Volume != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Volume);
      if (Url.Length != 0) hash ^= Url.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Playing != false) {
        output.WriteRawTag(8);
        output.WriteBool(Playing);
      }
      if (Volume != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Volume);
      }
      if (Url.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Url);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Playing != false) {
        size += 1 + 1;
      }
      if (Volume != 0F) {
        size += 1 + 4;
      }
      if (Url.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Url);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(PBAudioStream other) {
      if (other == null) {
        return;
      }
      if (other.Playing != false) {
        Playing = other.Playing;
      }
      if (other.Volume != 0F) {
        Volume = other.Volume;
      }
      if (other.Url.Length != 0) {
        Url = other.Url;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Playing = input.ReadBool();
            break;
          }
          case 21: {
            Volume = input.ReadFloat();
            break;
          }
          case 26: {
            Url = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code