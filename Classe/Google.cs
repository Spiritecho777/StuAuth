// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Google.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Migration {

  /// <summary>Holder for reflection information generated from Google.proto</summary>
  public static partial class GoogleReflection {

    #region Descriptor
    /// <summary>File descriptor for Google.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static GoogleReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgxHb29nbGUucHJvdG8SCW1pZ3JhdGlvbiKxBQoHUGF5bG9hZBI4Cg5vdHBf",
            "cGFyYW1ldGVycxgBIAMoCzIgLm1pZ3JhdGlvbi5QYXlsb2FkLk90cFBhcmFt",
            "ZXRlcnMSDwoHdmVyc2lvbhgCIAEoBRISCgpiYXRjaF9zaXplGAMgASgFEhMK",
            "C2JhdGNoX2luZGV4GAQgASgFEhAKCGJhdGNoX2lkGAUgASgFGp8ECg1PdHBQ",
            "YXJhbWV0ZXJzEg4KBnNlY3JldBgBIAEoDBIMCgRuYW1lGAIgASgJEg4KBmlz",
            "c3VlchgDIAEoCRI9CglhbGdvcml0aG0YBCABKA4yKi5taWdyYXRpb24uUGF5",
            "bG9hZC5PdHBQYXJhbWV0ZXJzLkFsZ29yaXRobRI7CgZkaWdpdHMYBSABKA4y",
            "Ky5taWdyYXRpb24uUGF5bG9hZC5PdHBQYXJhbWV0ZXJzLkRpZ2l0Q291bnQS",
            "NgoEdHlwZRgGIAEoDjIoLm1pZ3JhdGlvbi5QYXlsb2FkLk90cFBhcmFtZXRl",
            "cnMuT3RwVHlwZRIPCgdjb3VudGVyGAcgASgEInkKCUFsZ29yaXRobRIZChVB",
            "TEdPUklUSE1fVU5TUEVDSUZJRUQQABISCg5BTEdPUklUSE1fU0hBMRABEhQK",
            "EEFMR09SSVRITV9TSEEyNTYQAhIUChBBTEdPUklUSE1fU0hBNTEyEAMSEQoN",
            "QUxHT1JJVEhNX01ENRAEIlUKCkRpZ2l0Q291bnQSGwoXRElHSVRfQ09VTlRf",
            "VU5TUEVDSUZJRUQQABITCg9ESUdJVF9DT1VOVF9TSVgQARIVChFESUdJVF9D",
            "T1VOVF9FSUdIVBACIkkKB090cFR5cGUSGAoUT1RQX1RZUEVfVU5TUEVDSUZJ",
            "RUQQABIRCg1PVFBfVFlQRV9IT1RQEAESEQoNT1RQX1RZUEVfVE9UUBACYgZw",
            "cm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Migration.Payload), global::Migration.Payload.Parser, new[]{ "OtpParameters", "Version", "BatchSize", "BatchIndex", "BatchId" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::Migration.Payload.Types.OtpParameters), global::Migration.Payload.Types.OtpParameters.Parser, new[]{ "Secret", "Name", "Issuer", "Algorithm", "Digits", "Type", "Counter" }, null, new[]{ typeof(global::Migration.Payload.Types.OtpParameters.Types.Algorithm), typeof(global::Migration.Payload.Types.OtpParameters.Types.DigitCount), typeof(global::Migration.Payload.Types.OtpParameters.Types.OtpType) }, null, null)})
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class Payload : pb::IMessage<Payload>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Payload> _parser = new pb::MessageParser<Payload>(() => new Payload());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<Payload> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Migration.GoogleReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Payload() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Payload(Payload other) : this() {
      otpParameters_ = other.otpParameters_.Clone();
      version_ = other.version_;
      batchSize_ = other.batchSize_;
      batchIndex_ = other.batchIndex_;
      batchId_ = other.batchId_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Payload Clone() {
      return new Payload(this);
    }

    /// <summary>Field number for the "otp_parameters" field.</summary>
    public const int OtpParametersFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Migration.Payload.Types.OtpParameters> _repeated_otpParameters_codec
        = pb::FieldCodec.ForMessage(10, global::Migration.Payload.Types.OtpParameters.Parser);
    private readonly pbc::RepeatedField<global::Migration.Payload.Types.OtpParameters> otpParameters_ = new pbc::RepeatedField<global::Migration.Payload.Types.OtpParameters>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Migration.Payload.Types.OtpParameters> OtpParameters {
      get { return otpParameters_; }
    }

    /// <summary>Field number for the "version" field.</summary>
    public const int VersionFieldNumber = 2;
    private int version_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Version {
      get { return version_; }
      set {
        version_ = value;
      }
    }

    /// <summary>Field number for the "batch_size" field.</summary>
    public const int BatchSizeFieldNumber = 3;
    private int batchSize_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int BatchSize {
      get { return batchSize_; }
      set {
        batchSize_ = value;
      }
    }

    /// <summary>Field number for the "batch_index" field.</summary>
    public const int BatchIndexFieldNumber = 4;
    private int batchIndex_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int BatchIndex {
      get { return batchIndex_; }
      set {
        batchIndex_ = value;
      }
    }

    /// <summary>Field number for the "batch_id" field.</summary>
    public const int BatchIdFieldNumber = 5;
    private int batchId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int BatchId {
      get { return batchId_; }
      set {
        batchId_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as Payload);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(Payload other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!otpParameters_.Equals(other.otpParameters_)) return false;
      if (Version != other.Version) return false;
      if (BatchSize != other.BatchSize) return false;
      if (BatchIndex != other.BatchIndex) return false;
      if (BatchId != other.BatchId) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= otpParameters_.GetHashCode();
      if (Version != 0) hash ^= Version.GetHashCode();
      if (BatchSize != 0) hash ^= BatchSize.GetHashCode();
      if (BatchIndex != 0) hash ^= BatchIndex.GetHashCode();
      if (BatchId != 0) hash ^= BatchId.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      otpParameters_.WriteTo(output, _repeated_otpParameters_codec);
      if (Version != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Version);
      }
      if (BatchSize != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(BatchSize);
      }
      if (BatchIndex != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(BatchIndex);
      }
      if (BatchId != 0) {
        output.WriteRawTag(40);
        output.WriteInt32(BatchId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      otpParameters_.WriteTo(ref output, _repeated_otpParameters_codec);
      if (Version != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Version);
      }
      if (BatchSize != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(BatchSize);
      }
      if (BatchIndex != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(BatchIndex);
      }
      if (BatchId != 0) {
        output.WriteRawTag(40);
        output.WriteInt32(BatchId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += otpParameters_.CalculateSize(_repeated_otpParameters_codec);
      if (Version != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Version);
      }
      if (BatchSize != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(BatchSize);
      }
      if (BatchIndex != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(BatchIndex);
      }
      if (BatchId != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(BatchId);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(Payload other) {
      if (other == null) {
        return;
      }
      otpParameters_.Add(other.otpParameters_);
      if (other.Version != 0) {
        Version = other.Version;
      }
      if (other.BatchSize != 0) {
        BatchSize = other.BatchSize;
      }
      if (other.BatchIndex != 0) {
        BatchIndex = other.BatchIndex;
      }
      if (other.BatchId != 0) {
        BatchId = other.BatchId;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            otpParameters_.AddEntriesFrom(input, _repeated_otpParameters_codec);
            break;
          }
          case 16: {
            Version = input.ReadInt32();
            break;
          }
          case 24: {
            BatchSize = input.ReadInt32();
            break;
          }
          case 32: {
            BatchIndex = input.ReadInt32();
            break;
          }
          case 40: {
            BatchId = input.ReadInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            otpParameters_.AddEntriesFrom(ref input, _repeated_otpParameters_codec);
            break;
          }
          case 16: {
            Version = input.ReadInt32();
            break;
          }
          case 24: {
            BatchSize = input.ReadInt32();
            break;
          }
          case 32: {
            BatchIndex = input.ReadInt32();
            break;
          }
          case 40: {
            BatchId = input.ReadInt32();
            break;
          }
        }
      }
    }
    #endif

    #region Nested types
    /// <summary>Container for nested types declared in the Payload message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Types {
      [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
      public sealed partial class OtpParameters : pb::IMessage<OtpParameters>
      #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
          , pb::IBufferMessage
      #endif
      {
        private static readonly pb::MessageParser<OtpParameters> _parser = new pb::MessageParser<OtpParameters>(() => new OtpParameters());
        private pb::UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pb::MessageParser<OtpParameters> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Migration.Payload.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public OtpParameters() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public OtpParameters(OtpParameters other) : this() {
          secret_ = other.secret_;
          name_ = other.name_;
          issuer_ = other.issuer_;
          algorithm_ = other.algorithm_;
          digits_ = other.digits_;
          type_ = other.type_;
          counter_ = other.counter_;
          _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public OtpParameters Clone() {
          return new OtpParameters(this);
        }

        /// <summary>Field number for the "secret" field.</summary>
        public const int SecretFieldNumber = 1;
        private pb::ByteString secret_ = pb::ByteString.Empty;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public pb::ByteString Secret {
          get { return secret_; }
          set {
            secret_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "name" field.</summary>
        public const int NameFieldNumber = 2;
        private string name_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string Name {
          get { return name_; }
          set {
            name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "issuer" field.</summary>
        public const int IssuerFieldNumber = 3;
        private string issuer_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string Issuer {
          get { return issuer_; }
          set {
            issuer_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "algorithm" field.</summary>
        public const int AlgorithmFieldNumber = 4;
        private global::Migration.Payload.Types.OtpParameters.Types.Algorithm algorithm_ = global::Migration.Payload.Types.OtpParameters.Types.Algorithm.Unspecified;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public global::Migration.Payload.Types.OtpParameters.Types.Algorithm Algorithm {
          get { return algorithm_; }
          set {
            algorithm_ = value;
          }
        }

        /// <summary>Field number for the "digits" field.</summary>
        public const int DigitsFieldNumber = 5;
        private global::Migration.Payload.Types.OtpParameters.Types.DigitCount digits_ = global::Migration.Payload.Types.OtpParameters.Types.DigitCount.Unspecified;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public global::Migration.Payload.Types.OtpParameters.Types.DigitCount Digits {
          get { return digits_; }
          set {
            digits_ = value;
          }
        }

        /// <summary>Field number for the "type" field.</summary>
        public const int TypeFieldNumber = 6;
        private global::Migration.Payload.Types.OtpParameters.Types.OtpType type_ = global::Migration.Payload.Types.OtpParameters.Types.OtpType.Unspecified;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public global::Migration.Payload.Types.OtpParameters.Types.OtpType Type {
          get { return type_; }
          set {
            type_ = value;
          }
        }

        /// <summary>Field number for the "counter" field.</summary>
        public const int CounterFieldNumber = 7;
        private ulong counter_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public ulong Counter {
          get { return counter_; }
          set {
            counter_ = value;
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override bool Equals(object other) {
          return Equals(other as OtpParameters);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool Equals(OtpParameters other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (Secret != other.Secret) return false;
          if (Name != other.Name) return false;
          if (Issuer != other.Issuer) return false;
          if (Algorithm != other.Algorithm) return false;
          if (Digits != other.Digits) return false;
          if (Type != other.Type) return false;
          if (Counter != other.Counter) return false;
          return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override int GetHashCode() {
          int hash = 1;
          if (Secret.Length != 0) hash ^= Secret.GetHashCode();
          if (Name.Length != 0) hash ^= Name.GetHashCode();
          if (Issuer.Length != 0) hash ^= Issuer.GetHashCode();
          if (Algorithm != global::Migration.Payload.Types.OtpParameters.Types.Algorithm.Unspecified) hash ^= Algorithm.GetHashCode();
          if (Digits != global::Migration.Payload.Types.OtpParameters.Types.DigitCount.Unspecified) hash ^= Digits.GetHashCode();
          if (Type != global::Migration.Payload.Types.OtpParameters.Types.OtpType.Unspecified) hash ^= Type.GetHashCode();
          if (Counter != 0UL) hash ^= Counter.GetHashCode();
          if (_unknownFields != null) {
            hash ^= _unknownFields.GetHashCode();
          }
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void WriteTo(pb::CodedOutputStream output) {
        #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
          output.WriteRawMessage(this);
        #else
          if (Secret.Length != 0) {
            output.WriteRawTag(10);
            output.WriteBytes(Secret);
          }
          if (Name.Length != 0) {
            output.WriteRawTag(18);
            output.WriteString(Name);
          }
          if (Issuer.Length != 0) {
            output.WriteRawTag(26);
            output.WriteString(Issuer);
          }
          if (Algorithm != global::Migration.Payload.Types.OtpParameters.Types.Algorithm.Unspecified) {
            output.WriteRawTag(32);
            output.WriteEnum((int) Algorithm);
          }
          if (Digits != global::Migration.Payload.Types.OtpParameters.Types.DigitCount.Unspecified) {
            output.WriteRawTag(40);
            output.WriteEnum((int) Digits);
          }
          if (Type != global::Migration.Payload.Types.OtpParameters.Types.OtpType.Unspecified) {
            output.WriteRawTag(48);
            output.WriteEnum((int) Type);
          }
          if (Counter != 0UL) {
            output.WriteRawTag(56);
            output.WriteUInt64(Counter);
          }
          if (_unknownFields != null) {
            _unknownFields.WriteTo(output);
          }
        #endif
        }

        #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
          if (Secret.Length != 0) {
            output.WriteRawTag(10);
            output.WriteBytes(Secret);
          }
          if (Name.Length != 0) {
            output.WriteRawTag(18);
            output.WriteString(Name);
          }
          if (Issuer.Length != 0) {
            output.WriteRawTag(26);
            output.WriteString(Issuer);
          }
          if (Algorithm != global::Migration.Payload.Types.OtpParameters.Types.Algorithm.Unspecified) {
            output.WriteRawTag(32);
            output.WriteEnum((int) Algorithm);
          }
          if (Digits != global::Migration.Payload.Types.OtpParameters.Types.DigitCount.Unspecified) {
            output.WriteRawTag(40);
            output.WriteEnum((int) Digits);
          }
          if (Type != global::Migration.Payload.Types.OtpParameters.Types.OtpType.Unspecified) {
            output.WriteRawTag(48);
            output.WriteEnum((int) Type);
          }
          if (Counter != 0UL) {
            output.WriteRawTag(56);
            output.WriteUInt64(Counter);
          }
          if (_unknownFields != null) {
            _unknownFields.WriteTo(ref output);
          }
        }
        #endif

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int CalculateSize() {
          int size = 0;
          if (Secret.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeBytesSize(Secret);
          }
          if (Name.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
          }
          if (Issuer.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(Issuer);
          }
          if (Algorithm != global::Migration.Payload.Types.OtpParameters.Types.Algorithm.Unspecified) {
            size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Algorithm);
          }
          if (Digits != global::Migration.Payload.Types.OtpParameters.Types.DigitCount.Unspecified) {
            size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Digits);
          }
          if (Type != global::Migration.Payload.Types.OtpParameters.Types.OtpType.Unspecified) {
            size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Type);
          }
          if (Counter != 0UL) {
            size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Counter);
          }
          if (_unknownFields != null) {
            size += _unknownFields.CalculateSize();
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void MergeFrom(OtpParameters other) {
          if (other == null) {
            return;
          }
          if (other.Secret.Length != 0) {
            Secret = other.Secret;
          }
          if (other.Name.Length != 0) {
            Name = other.Name;
          }
          if (other.Issuer.Length != 0) {
            Issuer = other.Issuer;
          }
          if (other.Algorithm != global::Migration.Payload.Types.OtpParameters.Types.Algorithm.Unspecified) {
            Algorithm = other.Algorithm;
          }
          if (other.Digits != global::Migration.Payload.Types.OtpParameters.Types.DigitCount.Unspecified) {
            Digits = other.Digits;
          }
          if (other.Type != global::Migration.Payload.Types.OtpParameters.Types.OtpType.Unspecified) {
            Type = other.Type;
          }
          if (other.Counter != 0UL) {
            Counter = other.Counter;
          }
          _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void MergeFrom(pb::CodedInputStream input) {
        #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
          input.ReadRawMessage(this);
        #else
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                break;
              case 10: {
                Secret = input.ReadBytes();
                break;
              }
              case 18: {
                Name = input.ReadString();
                break;
              }
              case 26: {
                Issuer = input.ReadString();
                break;
              }
              case 32: {
                Algorithm = (global::Migration.Payload.Types.OtpParameters.Types.Algorithm) input.ReadEnum();
                break;
              }
              case 40: {
                Digits = (global::Migration.Payload.Types.OtpParameters.Types.DigitCount) input.ReadEnum();
                break;
              }
              case 48: {
                Type = (global::Migration.Payload.Types.OtpParameters.Types.OtpType) input.ReadEnum();
                break;
              }
              case 56: {
                Counter = input.ReadUInt64();
                break;
              }
            }
          }
        #endif
        }

        #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
                break;
              case 10: {
                Secret = input.ReadBytes();
                break;
              }
              case 18: {
                Name = input.ReadString();
                break;
              }
              case 26: {
                Issuer = input.ReadString();
                break;
              }
              case 32: {
                Algorithm = (global::Migration.Payload.Types.OtpParameters.Types.Algorithm) input.ReadEnum();
                break;
              }
              case 40: {
                Digits = (global::Migration.Payload.Types.OtpParameters.Types.DigitCount) input.ReadEnum();
                break;
              }
              case 48: {
                Type = (global::Migration.Payload.Types.OtpParameters.Types.OtpType) input.ReadEnum();
                break;
              }
              case 56: {
                Counter = input.ReadUInt64();
                break;
              }
            }
          }
        }
        #endif

        #region Nested types
        /// <summary>Container for nested types declared in the OtpParameters message type.</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static partial class Types {
          public enum Algorithm {
            [pbr::OriginalName("ALGORITHM_UNSPECIFIED")] Unspecified = 0,
            [pbr::OriginalName("ALGORITHM_SHA1")] Sha1 = 1,
            [pbr::OriginalName("ALGORITHM_SHA256")] Sha256 = 2,
            [pbr::OriginalName("ALGORITHM_SHA512")] Sha512 = 3,
            [pbr::OriginalName("ALGORITHM_MD5")] Md5 = 4,
          }

          public enum DigitCount {
            [pbr::OriginalName("DIGIT_COUNT_UNSPECIFIED")] Unspecified = 0,
            [pbr::OriginalName("DIGIT_COUNT_SIX")] Six = 1,
            [pbr::OriginalName("DIGIT_COUNT_EIGHT")] Eight = 2,
          }

          public enum OtpType {
            [pbr::OriginalName("OTP_TYPE_UNSPECIFIED")] Unspecified = 0,
            [pbr::OriginalName("OTP_TYPE_HOTP")] Hotp = 1,
            [pbr::OriginalName("OTP_TYPE_TOTP")] Totp = 2,
          }

        }
        #endregion

      }

    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code