# Charla "Mi aplicación no funciona y no se porqué"

En este repositorio está la aplicación de ejemplo usada para la charla así como la presentación de la misma.

La aplicación consiste de:

## Presentación

La presentación podéis descargarla de la carpeta **presentacion** de este mismo repositorio

## Arquitectura de la aplicación de ejemplo

<img src="images/ArquitecturaAplicación.png" style="zoom: 50%;" />

Es una aplicación muy sencilla con:

1. Un Frontal en Blazor en .Net 7.0.

   - Este frontal tiene 2 partes únicamente que son:

     1. Un catálogo de productos donde dandole al botón Buy  metes productos en un carrito de la compra.

        <img src="images/Productos.png" style="zoom:67%;" />

     2. Un carrito de la compra donde ves los productos que has dado a comprar.

   <img src="images/CestaCompra.png" style="zoom: 67%;" />

2. Una Api en .Net 7.0

   - Esta Api consume una base de datos en Azure SQL que obtiene un listado de productos.
   - Además se conecta con una cache de Redis donde se almacenan los productos de la compra.

## Arquitectura Observabilidad

<img src="images/ArquitecturaObservabilidad.png" style="zoom:67%;" />



Las aplicaciones envían la información Otel Collector.  Otel Collector se encarga de modificar, enriquecer modificar los datos recibidos, es decir las trazas , las métricas y los logs. Una vez realizados los cambios pertinentes Otel enviará la información a cada uno de los sistemas.

Estos sistemas son:

- En los logs vamos a usar Loki.
- En las métricas vamos a usar Prometheus.
- En el caso de las trazas  vamos a usar Jaeger.

Por último para poder consultar toda esta información de manera consolidada en un único sitio, usamos grafana.

Para tener más información sobre Otel Collector, y los demás sistemas consultar los siguientes enlaces:

- Otel Collector - https://opentelemetry.io/docs/collector/

- Loki - https://grafana.com/docs/loki/latest/

- Prometheus - https://opentelemetry.io/docs/collector/

- Jaeger - https://www.jaegertracing.io/docs/1.50/

- Grafana - https://grafana.com/grafana/

- 

Para poder levantar esta infraestructura de la manera más sencilla hay un Dockercompose en la ruta del repositorio **infrastructure-observability** .

```yaml
version: "2"
services:

  #Redis
  redis:
    image: redis/redis-stack-server:latest
    restart: always
    ports:
      - "6379:6379"
      
  # Jaeger
  jaeger-all-in-one:
    image: jaegertracing/all-in-one:latest
    restart: always
    ports:
      - "16686:16686"
      - "14268"
      - "14250"
      - "4327:4317"
    networks:
      - practical-otel-net  

# Loki
  loki:
    image: grafana/loki:latest
    command: [ "-config.file=/etc/loki/local-config.yaml" ] 
    networks:
      - practical-otel-net  

#Grafana
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    volumes:
      - ./grafana-datasources.yaml:/etc/grafana/provisioning/datasources/datasources.yaml
    environment:
      - GF_AUTH_ANONYMOUS_ENABLED=true
      - GF_AUTH_ANONYMOUS_ORG_ROLE=Admin
      - GF_AUTH_DISABLE_LOGIN_FORM=true
    depends_on:
      - jaeger-all-in-one
      - prometheus
      - loki
      - otel-collector
    networks:
      - practical-otel-net  

  # Collector
  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    restart: always
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml
    ports:
      - "1888:1888"   # pprof extension
      - "8888:8888"   # Prometheus metrics exposed by the collector
      - "8889:8889"   # Prometheus exporter metrics
      - "13133:13133" # health_check extension
      - "4317:4317"   # OTLP gRPC receiver
      - "55679:55679" # zpages extension
    depends_on:
      - jaeger-all-in-one
    networks:
      - practical-otel-net  

  prometheus:
    image: prom/prometheus:latest
    restart: always
    volumes:
      - ./prometheus.yaml:/etc/prometheus/prometheus.yml
    ports:
      - "9090:9090"
    networks:
      - practical-otel-net  

networks:
  practical-otel-net:
    name: practical-otel-net
```

  La configuración del Otel collector está disponible en el fichero **otel-collector-config.yaml** dentro también de la carpeta **infrastructure-observability** .

```yaml
receivers:
  otlp:
    protocols:
      grpc:

exporters:
  logging:
    loglevel: info

  prometheus:
    endpoint: "0.0.0.0:8889"
    const_labels:
      label1: value1

  debug:

  otlp:
    endpoint: jaeger-all-in-one:4317
    tls:
      insecure: true

  loki:
    endpoint: "http://loki:3100/loki/api/v1/push"
    tls:
      insecure: true
    
processors:
  attributes:
    actions:
    - action: insert
      key: loki.attribute.labels
      value: host.name
  
  resource:
    attributes:
    - action: insert
      key: loki.attribute.labels
      value: service.name
    - action: insert
      key: loki.resource.labels
      value: host.name,  service.instance.id
  batch:
  
extensions:
  health_check:
  pprof:
    endpoint: :1888
  zpages:
    endpoint: :55679

service:
  extensions: [pprof, zpages, health_check]
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [debug, otlp]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [debug, prometheus]
    logs:
      receivers: [otlp]
      processors: [batch]
      exporters: [logging, loki]

```

Para ver como crear este fichero de configuración, consultar el siguiente enlace: https://opentelemetry.io/docs/collector/configuration/ 

## Configurar OpenTelemetry en .Net Core

Para detalles de cómo configurar OpenTelemetry usar el siguiente enlace: https://opentelemetry.io/docs/instrumentation/net/manual/ 