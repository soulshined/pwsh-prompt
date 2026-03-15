Import-Module "$PSScriptRoot/../bin/Debug/pwsh-prompt/pwsh-prompt.psd1" -Force -DisableNameChecking -ErrorAction SilentlyContinue

$Databases = @(
	@{ Value = "Redis"; HotKey = "r"; HelpMessage = "Fast in-memory cache"; Description = "🔴 In-memory key-value store originally developed by Salvatore Sanfilippo.`nSupports pub/sub, streams, sorted sets, hyperloglogs, and geospatial indexes.`nIdeal for caching, session storage, real-time leaderboards, and rate limiting.`nCan be configured for persistence via RDB snapshots or AOF append-only files.`nRedis Cluster provides automatic partitioning across multiple nodes.`nSupports Lua scripting for atomic multi-step operations.`nRedis Sentinel provides high availability through automatic failover.`nThe RESP protocol makes it easy to build clients in any language.`nRecent versions added Redis Functions as a more structured alternative to Lua scripts.`nRedis Stack extends the core with modules for JSON, search, time series, and graph queries." }
	@{ Value = "PostgreSQL"; HotKey = "p"; HelpMessage = "Relational DB"; Description = "🐘 Advanced open-source relational database with over 35 years of active development.`nFull SQL compliance with powerful extensions like JSONB, arrays, and range types.`nExtensible with custom types, operators, functions, and extensions like PostGIS for geospatial data.`nMVCC concurrency model allows readers to never block writers and vice versa.`nSupports table partitioning, parallel queries, and JIT compilation for complex analytical workloads.`nBuilt-in logical and streaming replication for high availability setups.`nRich ecosystem of extensions: pg_stat_statements for query analysis, pgvector for AI embeddings 🤖, TimescaleDB for time series ⏱️.`nSupports row-level security policies for multi-tenant applications.`nFull-text search with tsvector and tsquery eliminates the need for external search engines in many cases." }
	@{ Value = "Azure Cosmos DB with Multi-Region Write and Automatic Failover"; HelpMessage = "Globally distributed, multi-model database service by Microsoft Azure with turnkey global distribution and five consistency models"; Description = "🌍 Fully managed NoSQL and relational database for modern app development.`nGuarantees single-digit millisecond reads and writes at the 99th percentile.`nOffers five well-defined consistency levels: strong, bounded staleness, session, consistent prefix, and eventual.`nMulti-model support including document, key-value, graph, and column-family via native APIs.`nAutomatic and instant scalability with serverless and provisioned throughput modes.`nIntegrated with Azure Functions, Azure Synapse Analytics, and Azure AI Search for end-to-end solutions." }
	@{ Value = "MongoDB"; HotKey = "m"; Description = "Document-oriented NoSQL database that stores data as flexible BSON documents.`nSchema-less design allows fields to vary between documents in the same collection.`nIdeal for unstructured or rapidly evolving schemas where relational modeling is impractical.`nSupports horizontal scaling via automatic sharding across commodity hardware.`nThe aggregation pipeline provides a powerful framework for data transformation and analysis.`nChange streams enable real-time reactive applications by watching for document modifications.`nAtlas Search integrates Apache Lucene-based full-text search directly into the database.`nMulti-document ACID transactions have been supported since version 4.0.`nTime series collections optimize storage and queries for IoT and metrics data." }
	@{ Value = "SQLite"; HelpMessage = "Embedded DB"; Description = "📦 Lightweight embedded relational database that runs in-process with zero configuration.`nNo separate server — the entire database is a single cross-platform file.`nPerfect for mobile apps 📱, CLI tools, desktop applications, and prototyping.`nUsed internally by every Android and iOS device, most web browsers, and countless embedded systems.`nSupports most of SQL92 with common table expressions, window functions, and JSON operators.`nWAL mode enables concurrent readers with a single writer for improved throughput.`nThe database file format is stable and backwards-compatible — files created in 2004 still work today.`nExtremely well-tested with 100% branch coverage and billions of tests in the test harness." }
	@{ Value = "MySQL"; HotKey = "y"; HelpMessage = "Popular relational DB" }
	@{ Value = "Cassandra"; Description = "Distributed wide-column NoSQL database designed for high availability with no single point of failure.`nExcels at write-heavy workloads across multiple data centers with tunable consistency levels.`nUses a masterless ring architecture where every node can accept reads and writes.`nCQL query language provides a familiar SQL-like interface for developers." }
	"DynamoDB"
	@{ Value = "Neo4j"; HotKey = "n"; HelpMessage = "Graph database"; Description = "🕸️ Native graph database for highly connected data.`nUses Cypher query language for expressive pattern matching across relationships.`nIdeal for social networks 👥, fraud detection 🔍, recommendation engines, and knowledge graphs.`nThe property graph model stores data as nodes, relationships, and properties on both.`nNative graph storage engine optimized for traversing relationships at constant time per hop.`nGraph Data Science library includes PageRank, community detection, node similarity, and pathfinding.`nAURA cloud service ☁️ provides fully managed Neo4j instances with automatic backups and scaling." }
	@{ Value = "MariaDB"; HelpMessage = "MySQL fork" }
	@{ Value = "CockroachDB"; Description = "Distributed SQL database built for global scale and survivability.`nAutomatic sharding, rebalancing, and repair with zero manual intervention.`nSerializable isolation by default — the strongest consistency guarantee available.`nGeo-partitioning pins data to specific regions for compliance and latency." }
	"Oracle Database"
	@{ Value = "SQL Server"; HotKey = "s"; HelpMessage = "Microsoft RDBMS"; Description = "Enterprise relational database with deep Windows and .NET integration.`nT-SQL extends standard SQL with powerful procedural programming features.`nAlways On Availability Groups provide high availability and disaster recovery.`nColumnstore indexes enable blazing-fast analytical queries on OLTP databases.`nSQL Server on Linux 🐧 brings the engine to non-Windows environments." }
	@{ Value = "Elasticsearch"; HelpMessage = "Full-text search engine"; Description = "🔍 Distributed search and analytics engine built on Apache Lucene.`nNear real-time search across structured and unstructured data.`nPowerful aggregation framework for complex analytics and metrics.`nPart of the Elastic Stack with Kibana, Logstash, and Beats for observability." }
	@{ Value = "InfluxDB"; Description = "Purpose-built time series database for metrics, events, and real-time analytics.`nOptimized storage engine for high write throughput and compressed time series data.`nFlux query language provides functional transformations over time-windowed data.`nIntegrates with Grafana, Telegraf, and Kapacitor for monitoring pipelines." }
	"Memcached"
	@{ Value = "CouchDB"; HotKey = "c"; HelpMessage = "Multi-master replication" }
	@{ Value = "Firestore"; Description = "☁️ Serverless document database from Google Cloud with real-time sync.`nAutomatic scaling from zero to millions of concurrent connections.`nOffline support for mobile and web apps with local caching and sync.`nStrong consistency for single-document operations, eventual for queries." }
	@{ Value = "Snowflake"; HelpMessage = "Cloud data warehouse"; Description = "❄️ Cloud-native data warehouse with separation of storage and compute.`nVirtual warehouses can scale independently for concurrent workloads.`nNear-zero maintenance with automatic clustering, optimization, and encryption.`nSecure data sharing across organizations without copying data.`nTime travel and fail-safe features for data recovery and auditing." }
	"Amazon Aurora"
	@{ Value = "Apache HBase"; Description = "Wide-column store built on top of HDFS for random real-time read/write access.`nModeled after Google Bigtable for massive sparse datasets.`nIntegrates with the Hadoop ecosystem for MapReduce and Spark processing." }
	@{ Value = "Couchbase"; HotKey = "b"; HelpMessage = "Distributed engagement DB" }
	@{ Value = "TiDB"; Description = "Distributed NewSQL database compatible with the MySQL protocol.`nHTAP architecture handles both transactional and analytical workloads.`nElastic horizontal scaling with online DDL changes and zero downtime." }
	"Teradata"
	@{ Value = "ClickHouse"; HotKey = "k"; HelpMessage = "Columnar OLAP DB"; Description = "Blazing-fast open-source columnar database for real-time analytical queries.`nVector execution engine processes billions of rows per second on commodity hardware.`nAggressive compression reduces storage costs while maintaining query speed.`nMaterialized views enable pre-aggregated dashboards with sub-second response." }
	"Db2"
	@{ Value = "Supabase"; HelpMessage = "Open-source Firebase alternative"; Description = "🟢 Backend-as-a-service built on PostgreSQL with real-time subscriptions.`nAuto-generated REST and GraphQL APIs from your database schema.`nBuilt-in auth, storage, and edge functions for full-stack development.`nRow-level security policies inherited from PostgreSQL." }
	@{ Value = "PlanetScale"; Description = "Serverless MySQL-compatible database platform powered by Vitess.`nNon-blocking schema changes with deploy requests and safe migrations.`nBranching workflow lets you test schema changes like Git branches." }
	"Apache Derby"
	@{ Value = "Fauna"; HotKey = "f"; HelpMessage = "Serverless document-relational DB"; Description = "🦎 Distributed document-relational database with native GraphQL.`nACID transactions across globally distributed data without configuration.`nTemporality built in — query historical snapshots of any document.`nServerless pricing with zero operational overhead." }
	@{ Value = "TimescaleDB"; Description = "⏱️ PostgreSQL extension optimized for time series workloads.`nAutomatic partitioning via hypertables for write performance at scale.`nContinuous aggregates maintain real-time materialized views.`nFull SQL support with time-series-specific functions and hyperfunctions." }
	"Informix"
	@{ Value = "VoltDB"; HelpMessage = "In-memory ACID database" }
	@{ Value = "RavenDB"; HotKey = "v"; Description = "Fully transactional NoSQL document database with an integrated search engine.`nAutomatic indexing creates and manages indexes based on query patterns.`nDistributed counters and time series as native data types.`nMulti-document ACID transactions with optimistic concurrency by default." }
	@{ Value = "ScyllaDB"; HelpMessage = "C++ Cassandra replacement"; Description = "High-performance NoSQL database compatible with Cassandra at 10x the throughput.`nWritten in C++ with a shard-per-core architecture for predictable latency.`nAutomatic tuning eliminates the need for manual JVM garbage collection tuning." }
	"Ingres"
	@{ Value = "MindsDB"; Description = "🤖 AI-powered database that brings machine learning to existing data stores.`nCreate, train, and deploy models using SQL syntax.`nConnects to 100+ data sources including databases, warehouses, and SaaS apps." }
	@{ Value = "SurrealDB"; HotKey = "u"; HelpMessage = "Multi-model database" }
	"Greenplum"
	@{ Value = "QuestDB"; Description = "High-performance time series database with SQL support.`nIngests millions of rows per second via ILP protocol.`nSIMD-accelerated execution for fast aggregations over time ranges." }
	@{ Value = "Apache Druid"; HelpMessage = "Real-time analytics"; Description = "🐉 Column-oriented distributed data store for sub-second OLAP queries.`nReal-time ingestion with immediate queryability for streaming data.`nAutomatic data summarization and rollup for storage efficiency." }
	@{ Value = "Yugabyte"; HotKey = "g"; Description = "Distributed SQL database combining PostgreSQL compatibility with horizontal scale.`nAutomatic sharding and rebalancing across nodes.`nGlobal transactions with configurable read replicas." }
	"Apache Ignite"
	@{ Value = "DuckDB"; HelpMessage = "In-process OLAP"; Description = "🦆 In-process analytical database with a rich SQL dialect.`nRuns embedded like SQLite but optimized for analytical workloads.`nDirectly queries Parquet, CSV, and JSON files without importing.`nColumnar vectorized execution engine for fast aggregations.`nZero dependencies and single-file deployment." }
	@{ Value = "Neon"; Description = "Serverless PostgreSQL with branching and scale-to-zero.`nInstant database branches for development and testing workflows.`nSeparated storage and compute with autoscaling replicas." }
	"SAP HANA"
	@{ Value = "Valkey"; HotKey = "l"; HelpMessage = "Open-source Redis fork" }
	@{ Value = "Milvus"; Description = "🧠 Open-source vector database purpose-built for AI similarity search.`nSupports billion-scale vector search with multiple index types.`nHybrid search combining vector similarity with scalar filtering.`nCloud-native architecture with separation of storage and compute." }
	"Apache Pinot"
	@{ Value = "EdgeDB"; HelpMessage = "Graph-relational database"; Description = "Object-relational database with a novel query language called EdgeQL.`nBuilt on top of PostgreSQL with a strict type system.`nMigration system tracks schema changes declaratively." }
	@{ Value = "KeyDB"; Description = "Multi-threaded fork of Redis with higher throughput on multi-core hardware.`nDrop-in replacement maintaining full Redis API compatibility.`nActive replication for sub-millisecond failover between nodes." }
	"Pervasive PSQL"
	@{ Value = "FoundationDB"; HotKey = "d"; HelpMessage = "Ordered key-value store"; Description = "Distributed ordered key-value store with strict ACID transactions.`nUsed as the storage backend for Apple's CloudKit and Snowflake.`nLayered architecture allows building any data model on top.`nSimulation testing validates correctness under all failure modes." }
	@{ Value = "Weaviate"; Description = "🔮 AI-native vector database with built-in vectorization modules.`nAutomatically vectorizes data using OpenAI, Cohere, or Hugging Face 🤗 models.`nGraphQL and REST APIs with hybrid vector and keyword search.`nMulti-tenancy support for SaaS applications." }
	"Riak"
	@{ Value = "TDengine"; HotKey = "t"; HelpMessage = "IoT time series DB" }
	@{ Value = "Qdrant"; Description = "High-performance vector similarity search engine with filtering.`nWritten in Rust for memory safety and speed.`nPayload filtering combines vector search with metadata constraints.`nDistributed deployment with Raft consensus for reliability." }
	"MaxDB"
	@{ Value = "ArangoDB"; HelpMessage = "Multi-model DB"; Description = "🥑 Native multi-model database supporting documents, graphs, and key-value.`nAQL query language works across all data models uniformly.`nSmartGraphs distribute graph data for enterprise-scale traversals.`nFoxx microservices framework runs JavaScript directly in the database." }
	@{ Value = "Tigris"; Description = "Open-source serverless NoSQL database and search platform.`nAutomatic database provisioning with a developer-friendly TypeScript SDK.`nReal-time search powered by embedded full-text indexing." }
	"SQLBase"
	"Empress"
)

$Result = Prompt-Choice $Databases `
	-Message @{ Text = "Pick a database for the project"; Style = "Bold" } `
	-Title @{ Text = "Database Picker"; Style = "Bold,Underline" } `
	-AlternateBuffer @{} `
	-Multiple

if ($null -eq $Result) {
	Write-Host "`n  Cancelled!" -ForegroundColor DarkGray
	return
}

$Db = $Databases[$Result[0]]
$Name = if ($Db -is [hashtable]) { $Db.Value } else { $Db }
Write-Host ""
Write-Host "  Selected: $Name" -ForegroundColor Green
Write-Host ""
