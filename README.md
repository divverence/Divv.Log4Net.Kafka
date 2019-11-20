# Divv.Log4Net

A set of plugings for log4net that allow direct structured logging to Kafka in json format.

## Components

### Divv.Log4Net.Kafka

Provides a log4net appender that uses Confluent's official Kafka producer client to log directly to Kafka, in any desired layout.

## Divv.Log4Net.Json

Provides a log4net layout that wraps the message in a json object, together with both a fixed set of properties, all log4net context, a set of configurable tags and a selection of contextVariables.

## Usage

### Nuget package

Not yet available.

### ZIP package

This package is meant to be used as a 3rd party plugin for log4net applications that didn't ship with Kafka or Json support.

The ZIP file contains the two libraries, and the subdirectory with native kafka dlls that are required for the Kafka plugin to function.

All managed dependencies excluding log4net itself have been ILMerged into the dlls to reduce any possible conflict surface with pre-existing libraries shipped with the application.

#### Installation 

Download the zip file containing the assemblies and native dependencies, extract it to the application directory to where the log4net dll is present.

Make sure no other (and possibly incompatible) versions of librdkafka native libraries are present in this directory, as they appear to take precedence over the included set, and might be incompatible.

## Configuring log4net sections

Loading the appender is done by referencing the full type string `type="Divv.Log4Net.Kafka.KafkaAppender, Divv.Log4Net.Kafka"`; In a similar fashion, the json layout can be loaded using `type="Divv.Log4Net.Json.JsonLayout, Divv.Log4Net.Json"`.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<log4net>

  <!-- Add the KafkaAppender -->

  <appender name="KafkaAppender" type="Divv.Log4Net.Kafka.KafkaAppender, Divv.Log4Net.Kafka">

    <KafkaSettings>
      <brokers>
        <add value="kafka-broker-1:9092" />
        <add value="kafka-broker-2:9092" />
      </brokers>
      <topic type="log4net.Layout.PatternLayout">
        <conversionPattern value="productionlogs" /><!-- You can use. eg loglevel, logger etc to use other topics per message -->
      </topic>
    </KafkaSettings>

    <layout type="Divv.Log4Net.Json.JsonLayout, Divv.Log4Net.Json" >
      <app value="erp.logs" />
      <tags value="seachange,traxis" />
      <callcontextvariables value="question,answer" />
    </layout>

  </appender>

  <!-- Include the added appender to the root (or custom) appenders list -->

  <root>
    <level value="DEBUG"/>
    <appender-ref ref="KafkaAppender" />
  </root>

</log4net>
```

### Alternative modes of operation

Of course, you don't have to publish json objects to Kafka; you can use any layouter you like (such as PatternLayout). You can also use the Json Layout with other appenders, such as a UDP appender.
