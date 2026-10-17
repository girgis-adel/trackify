using Common.Core.Properties;

namespace Common.Core.Abstractions;

public interface IEntity : IId, ICreateDate, IUpdateDate, IVersion;
