# VIEApps.Enyim.Caching.Transcoders

- The custom transcoders of [VIEApps.Enyim.Caching](https://github.com/vieapps/Enyim.Caching).

- Its serializes object using [Protocol Buffers](http://code.google.com/p/protobuf-net/), [MessagePack](https://github.com/msgpack/msgpack-cli) and BSON.

## NuGet

[![NuGet](https://img.shields.io/nuget/v/VIEApps.Enyim.Caching.Transcoders.svg)](https://www.nuget.org/packages/VIEApps.Enyim.Caching.Transcoders)

## Configuration

### The appsettings.json file

```json
{
	"Memcached": {
		"Servers": [
			{
				"Address": "127.0.0.1",
				"Port": 11211
			}
		],
		"Transcoder": "Enyim.Caching.Memcached.ProtocolBuffersTranscoder, Enyim.Caching.Transcoders"
	}
}
```

### The app.config/web.config file 

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<configSections>
		<section name="memcached" type="Enyim.Caching.Configuration.MemcachedClientConfigurationSectionHandler, Enyim.Caching" />
	</configSections>
	<memcached>
		<servers>
			<add address="127.0.0.1" port="11211" />
		</servers>
		<socketPool minPoolSize="10" maxPoolSize="100" deadTimeout="00:01:00" connectionTimeout="00:00:05" receiveTimeout="00:00:01" />
		<transcoder type="Enyim.Caching.Memcached.ProtocolBuffersTranscoder, Enyim.Caching.Transcoders" />
	</memcached>
</configuration>
```

## Available transcoders

- Default (MsgPack.CLI): `Enyim.Caching.Memcached.DefaultTranscoder,Enyim.Caching`
- BSON: `Enyim.Caching.Memcached.BsonTranscoder,Enyim.Caching.Transcoders`
- Protocol Buffers: `Enyim.Caching.Memcached.ProtocolBuffersTranscoder,Enyim.Caching.Transcoders`
- Message Pack Array mode: `Enyim.Caching.Memcached.MessagePackArrayTranscoder,Enyim.Caching.Transcoders`
- Message Pack Map mode: `Enyim.Caching.Memcached.MessagePackMapTranscoder,Enyim.Caching.Transcoders`

## Performance

See the results of <b>neuecc</b> at https://github.com/neuecc/MemcachedTranscoder/blob/master/ReadMe.md