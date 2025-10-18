using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class WebSocketMessage
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("source")]
    public string Source { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    [JsonProperty("data")]
    public JObject Data { get; set; }
}

[Serializable]
public class ProjectStructure
{
    [JsonProperty("projectName")]
    public string ProjectName { get; set; }

    [JsonProperty("packages")]
    public List<PackageNode> Packages { get; set; }

    public ProjectStructure()
    {
        ProjectName = string.Empty;
        Packages = new List<PackageNode>();
    }

    public ProjectStructure(string projectName, List<PackageNode> packages)
    {
        ProjectName = projectName;
        Packages = packages;
        BuildHierarchy();
    }

    public static ProjectStructure CreateDummy()
    {
        var dummy = new ProjectStructure("ar-3d-visual-city", new List<PackageNode>
    {
        new PackageNode(
            "",
            new List<FileNode>
            {
                new FileNode(
                    "Launcher.class",
                    "C:/Projects/ar-3d-visual-city/out/production/Launcher.class",
                    "AnotherPackage",
                    0,
                    14,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "Launcher",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            14,
                            2,
                            1,
                            new List<MethodNode>
                            {
                                new MethodNode("Launcher", null, new List<string>(), new List<string>(), new List<string>{"public"}, 2),
                                new MethodNode("main", "void", new List<string>{"String[]"}, new List<string>(), new List<string>{"public","static"}, 8)
                            },
                            new List<FieldNode>
                            {
                                new FieldNode("appVersion","String",null,new List<string>{"private","static"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),

        new PackageNode(
            "MainPackage",
            new List<FileNode>
            {
                new FileNode(
                    "Main.java",
                    "C:/Projects/ar-3d-visual-city/src/MainPackage/Main.java",
                    "MainPackage",
                    1,
                    30,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "Main",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            30,
                            2,
                            1,
                            new List<MethodNode>
                            {
                                new MethodNode("init","void",new List<string>(),new List<string>(),new List<string>{"private"},4),
                                new MethodNode("main","void",new List<string>{"String[]"},new List<string>(),new List<string>{"public","static"},12)
                            },
                            new List<FieldNode>{ new FieldNode("initialized","boolean",null,new List<string>{"private"}) },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),

        new PackageNode(
            "Utils",
            new List<FileNode>
            {
                new FileNode(
                    "FileUtils.java",
                    "C:/Projects/ar-3d-visual-city/src/Utils/FileUtils.java",
                    "Utils",
                    3,
                    57,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "FileUtils",
                            false,
                            false,
                            new List<string>{"public","final"},
                            "java.lang.Object",
                            new List<string>(),
                            52,
                            4,
                            2,
                            new List<MethodNode>
                            {
                                new MethodNode("readAllLines","List<String>",new List<string>{"File"},new List<string>(),new List<string>{"public","static"},20),
                                new MethodNode("writeText","void",new List<string>{"File","String"},new List<string>(),new List<string>{"public","static"},15),
                                new MethodNode("copy","void",new List<string>{"File","File"},new List<string>(),new List<string>{"public","static"},12),
                                new MethodNode("delete","boolean",new List<string>{"File"},new List<string>(),new List<string>{"public","static"},10)
                            },
                            new List<FieldNode>{
                                new FieldNode("BUFFER_SIZE","int",null,new List<string>{"public","static","final"}),
                                new FieldNode("DEFAULT_ENCODING","String",null,new List<string>{"public","static","final"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "Utils.Parsing",
            new List<FileNode>
            {
                new FileNode(
                    "JsonParser.java",
                    "C:/Projects/ar-3d-visual-city/src/Utils/Parsing/JsonParser.java",
                    "Utils.Parsing",
                    2,
                    75,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "JsonParser",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            70,
                            5,
                            3,
                            new List<MethodNode>
                            {
                                new MethodNode("parseObject","JsonNode",new List<string>{"String"},new List<string>(),new List<string>{"public"},25),
                                new MethodNode("parseArray","List<JsonNode>",new List<string>{"String"},new List<string>(),new List<string>{"public"},20),
                                new MethodNode("validate","boolean",new List<string>{"String"},new List<string>(),new List<string>{"public"},10),
                                new MethodNode("fromFile","JsonNode",new List<string>{"File"},new List<string>(),new List<string>{"public"},8),
                                new MethodNode("toPrettyString","String",new List<string>{"JsonNode"},new List<string>(),new List<string>{"public"},5)
                            },
                            new List<FieldNode>{
                                new FieldNode("mapper","ObjectMapper",null,new List<string>{"private"}),
                                new FieldNode("strictMode","boolean",null,new List<string>{"protected","static"}),
                                new FieldNode("lastError","String",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                ),
                new FileNode(
                    "XmlParser.java",
                    "C:/Projects/ar-3d-visual-city/src/Utils/Parsing/XmlParser.java",
                    "Utils.Parsing",
                    3,
                    67,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "XmlParser",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            65,
                            3,
                            2,
                            new List<MethodNode>
                            {
                                new MethodNode("fromFile","Document",new List<string>{"File"},new List<string>(),new List<string>{"public"},15),
                                new MethodNode("fromString","Document",new List<string>{"String"},new List<string>(),new List<string>{"public"},10),
                                new MethodNode("toString","String",new List<string>{"Document"},new List<string>(),new List<string>{"public"},8)
                            },
                            new List<FieldNode>{
                                new FieldNode("builderFactory","DocumentBuilderFactory",null,new List<string>{"private","static"}),
                                new FieldNode("lastDoc","Document",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),

        new PackageNode(
            "Networking",
            new List<FileNode>
            {
                new FileNode(
                    "NetworkManager.java",
                    "C:/Projects/ar-3d-visual-city/src/Networking/NetworkManager.java",
                    "Networking",
                    4,
                    82,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "NetworkManager",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>{"Runnable"},
                            75,
                            3,
                            3,
                            new List<MethodNode>
                            {
                                new MethodNode("start","void",new List<string>(),new List<string>(),new List<string>{"public"},15),
                                new MethodNode("stop","void",new List<string>(),new List<string>(),new List<string>{"public"},10),
                                new MethodNode("run","void",new List<string>(),new List<string>(),new List<string>{"public"},10)
                            },
                            new List<FieldNode>{
                                new FieldNode("socket","Socket",null,new List<string>{"private"}),
                                new FieldNode("running","boolean",null,new List<string>{"private"}),
                                new FieldNode("id","int",null,new List<string>{"private","final"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "Networking.Http",
            new List<FileNode>
            {
                new FileNode(
                    "HttpClient.java",
                    "C:/Projects/ar-3d-visual-city/src/Networking/Http/HttpClient.java",
                    "Networking.Http",
                    5,
                    135,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "HttpClient",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>{"Closeable"},
                            130,
                            6,
                            3,
                            new List<MethodNode>
                            {
                                new MethodNode("get","Response",new List<string>{"String"},new List<string>(),new List<string>{"public"},25),
                                new MethodNode("post","Response",new List<string>{"String","String"},new List<string>(),new List<string>{"public"},30),
                                new MethodNode("put","Response",new List<string>{"String","String"},new List<string>(),new List<string>{"public"},25),
                                new MethodNode("delete","Response",new List<string>{"String"},new List<string>(),new List<string>{"public"},20),
                                new MethodNode("close","void",new List<string>(),new List<string>(),new List<string>{"public"},5),
                                new MethodNode("setAuthToken","void",new List<string>{"String"},new List<string>(),new List<string>{"public"},6)
                            },
                            new List<FieldNode>{
                                new FieldNode("headers","Map<String,String>",null,new List<string>{"private"}),
                                new FieldNode("timeoutMs","int",null,new List<string>{"private"}),
                                new FieldNode("authToken","String",null,new List<string>{"private"})
                            },
                            new List<ClassNode>{
                                new ClassNode(
                                    "Response",
                                    false,
                                    false,
                                    new List<string>{"public","static"},
                                    "java.lang.Object",
                                    new List<string>(),
                                    30,
                                    2,
                                    2,
                                    new List<MethodNode>{
                                        new MethodNode("getStatusCode","int",new List<string>(),new List<string>(),new List<string>{"public"},6),
                                        new MethodNode("getBody","String",new List<string>(),new List<string>(),new List<string>{"public"},8)
                                    },
                                    new List<FieldNode>{
                                        new FieldNode("statusCode","int",null,new List<string>{"private"}),
                                        new FieldNode("body","String",null,new List<string>{"private"})
                                    },
                                    new List<ClassNode>()
                                )
                            }
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "Networking.WebSocket",
            new List<FileNode>
            {
                new FileNode(
                    "WebSocketHandler.java",
                    "C:/Projects/ar-3d-visual-city/src/Networking/WebSocket/WebSocketHandler.java",
                    "Networking.WebSocket",
                    4,
                    92,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "WebSocketHandler",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            86,
                            4,
                            2,
                            new List<MethodNode>
                            {
                                new MethodNode("connect","void",new List<string>{"String"},new List<string>(),new List<string>{"public"},20),
                                new MethodNode("onMessage","void",new List<string>{"String"},new List<string>(),new List<string>{"protected"},10),
                                new MethodNode("send","void",new List<string>{"String"},new List<string>(),new List<string>{"public"},15),
                                new MethodNode("close","void",new List<string>(),new List<string>(),new List<string>{"public"},5)
                            },
                            new List<FieldNode>{
                                new FieldNode("connected","boolean",null,new List<string>{"private"}),
                                new FieldNode("endpoint","String",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),

        new PackageNode(
            "Data",
            new List<FileNode>
            {
                new FileNode(
                    "DatabaseConnection.java",
                    "C:/Projects/ar-3d-visual-city/src/Data/DatabaseConnection.java",
                    "Data",
                    3,
                    95,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "DatabaseConnection",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            90,
                            4,
                            3,
                            new List<MethodNode>
                            {
                                new MethodNode("connect","void",new List<string>(),new List<string>(),new List<string>{"public"},20),
                                new MethodNode("disconnect","void",new List<string>(),new List<string>(),new List<string>{"public"},10),
                                new MethodNode("isConnected","boolean",new List<string>(),new List<string>(),new List<string>{"public"},5),
                                new MethodNode("executeQuery","ResultSet",new List<string>{"String"},new List<string>(),new List<string>{"public"},30)
                            },
                            new List<FieldNode>{
                                new FieldNode("url","String",null,new List<string>{"private"}),
                                new FieldNode("username","String",null,new List<string>{"private"}),
                                new FieldNode("password","String",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "Data.Repositories",
            new List<FileNode>
            {
                new FileNode(
                    "UserRepository.java",
                    "C:/Projects/ar-3d-visual-city/src/Data/Repositories/UserRepository.java",
                    "Data.Repositories",
                    3,
                    86,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "UserRepository",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            80,
                            4,
                            2,
                            new List<MethodNode>
                            {
                                new MethodNode("findById","User",new List<string>{"int"},new List<string>(),new List<string>{"public"},10),
                                new MethodNode("findAll","List<User>",new List<string>(),new List<string>(),new List<string>{"public"},12),
                                new MethodNode("save","void",new List<string>{"User"},new List<string>(),new List<string>{"public"},20),
                                new MethodNode("delete","void",new List<string>{"int"},new List<string>(),new List<string>{"public"},15)
                            },
                            new List<FieldNode>{
                                new FieldNode("connection","DatabaseConnection",null,new List<string>{"private"}),
                                new FieldNode("cache","Map<Integer,User>",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "Data.Entities",
            new List<FileNode>
            {
                new FileNode(
                    "Building.java",
                    "C:/Projects/ar-3d-visual-city/src/Data/Entities/Building.java",
                    "Data.Entities",
                    2,
                    50,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "Building",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            45,
                            3,
                            3,
                            new List<MethodNode>
                            {
                                new MethodNode("getId","int",new List<string>(),new List<string>(),new List<string>{"public"},5),
                                new MethodNode("getName","String",new List<string>(),new List<string>(),new List<string>{"public"},5),
                                new MethodNode("setName","void",new List<string>{"String"},new List<string>(),new List<string>{"public"},8)
                            },
                            new List<FieldNode>{
                                new FieldNode("id","int",null,new List<string>{"private"}),
                                new FieldNode("name","String",null,new List<string>{"private"}),
                                new FieldNode("height","double",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),

        new PackageNode(
            "Core",
            new List<FileNode>
            {
                new FileNode(
                    "ApplicationCore.java",
                    "C:/Projects/ar-3d-visual-city/src/Core/ApplicationCore.java",
                    "Core",
                    4,
                    102,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "ApplicationCore",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            98,
                            5,
                            3,
                            new List<MethodNode>
                            {
                                new MethodNode("initialize","void",new List<string>(),new List<string>(),new List<string>{"public"},20),
                                new MethodNode("loadModules","void",new List<string>(),new List<string>(),new List<string>{"private"},25),
                                new MethodNode("shutdown","void",new List<string>(),new List<string>(),new List<string>{"public"},15),
                                new MethodNode("getModuleCount","int",new List<string>(),new List<string>(),new List<string>{"public"},6),
                                new MethodNode("getName","String",new List<string>(),new List<string>(),new List<string>{"public"},5)
                            },
                            new List<FieldNode>{
                                new FieldNode("modules","List<Module>",null,new List<string>{"private"}),
                                new FieldNode("name","String",null,new List<string>{"private","final"}),
                                new FieldNode("initialized","boolean",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "Core.Config",
            new List<FileNode>
            {
                new FileNode(
                    "AppConfig.java",
                    "C:/Projects/ar-3d-visual-city/src/Core/Config/AppConfig.java",
                    "Core.Config",
                    2,
                    68,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "AppConfig",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            65,
                            3,
                            2,
                            new List<MethodNode>
                            {
                                new MethodNode("load","void",new List<string>(),new List<string>(),new List<string>{"public"},15),
                                new MethodNode("get","String",new List<string>{"String"},new List<string>(),new List<string>{"public"},8),
                                new MethodNode("set","void",new List<string>{"String","String"},new List<string>(),new List<string>{"public"},8)
                            },
                            new List<FieldNode>{
                                new FieldNode("properties","Map<String,String>",null,new List<string>{"private"}),
                                new FieldNode("fileLoaded","boolean",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),

        new PackageNode(
            "Controllers",
            new List<FileNode>
            {
                new FileNode(
                    "SceneController.java",
                    "C:/Projects/ar-3d-visual-city/src/Controllers/SceneController.java",
                    "Controllers",
                    3,
                    77,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "SceneController",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            72,
                            3,
                            2,
                            new List<MethodNode>
                            {
                                new MethodNode("loadScene","void",new List<string>{"String"},new List<string>(),new List<string>{"public"},20),
                                new MethodNode("unloadScene","void",new List<string>(),new List<string>(),new List<string>{"public"},15),
                                new MethodNode("reload","void",new List<string>(),new List<string>(),new List<string>{"public"},10)
                            },
                            new List<FieldNode>{
                                new FieldNode("activeScene","String",null,new List<string>{"private"}),
                                new FieldNode("loadedScenes","List<String>",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "Services",
            new List<FileNode>
            {
                new FileNode(
                    "AnalyticsService.java",
                    "C:/Projects/ar-3d-visual-city/src/Services/AnalyticsService.java",
                    "Services",
                    4,
                    88,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "AnalyticsService",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            83,
                            4,
                            3,
                            new List<MethodNode>
                            {
                                new MethodNode("trackEvent","void",new List<string>{"String","Map<String,String>"},new List<string>(),new List<string>{"public"},20),
                                new MethodNode("getEventCount","int",new List<string>(),new List<string>(),new List<string>{"public"},5),
                                new MethodNode("flush","void",new List<string>(),new List<string>(),new List<string>{"public"},8),
                                new MethodNode("reset","void",new List<string>(),new List<string>(),new List<string>{"public"},6)
                            },
                            new List<FieldNode>{
                                new FieldNode("events","List<Event>",null,new List<string>{"private"}),
                                new FieldNode("enabled","boolean",null,new List<string>{"private"}),
                                new FieldNode("maxBatchSize","int",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "UI",
            new List<FileNode>
            {
                new FileNode(
                    "MainWindow.java",
                    "C:/Projects/ar-3d-visual-city/src/UI/MainWindow.java",
                    "UI",
                    5,
                    112,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "MainWindow",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            105,
                            5,
                            3,
                            new List<MethodNode>
                            {
                                new MethodNode("open","void",new List<string>(),new List<string>(),new List<string>{"public"},15),
                                new MethodNode("close","void",new List<string>(),new List<string>(),new List<string>{"public"},10),
                                new MethodNode("render","void",new List<string>(),new List<string>(),new List<string>{"public"},30),
                                new MethodNode("resize","void",new List<string>{"int","int"},new List<string>(),new List<string>{"public"},8),
                                new MethodNode("onEvent","void",new List<string>{"String"},new List<string>(),new List<string>{"protected"},12)
                            },
                            new List<FieldNode>{
                                new FieldNode("width","int",null,new List<string>{"private"}),
                                new FieldNode("height","int",null,new List<string>{"private"}),
                                new FieldNode("title","String",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),

        new PackageNode(
            "Analytics",
            new List<FileNode>
            {
                new FileNode(
                    "PerformanceTracker.java",
                    "C:/Projects/ar-3d-visual-city/src/Analytics/PerformanceTracker.java",
                    "Analytics",
                    3,
                    96,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "PerformanceTracker",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            92,
                            4,
                            2,
                            new List<MethodNode>
                            {
                                new MethodNode("startTracking","void",new List<string>(),new List<string>(),new List<string>{"public"},12),
                                new MethodNode("stopTracking","void",new List<string>(),new List<string>(),new List<string>{"public"},10),
                                new MethodNode("getAverageMs","double",new List<string>(),new List<string>(),new List<string>{"public"},10),
                                new MethodNode("reset","void",new List<string>(),new List<string>(),new List<string>{"public"},5)
                            },
                            new List<FieldNode>{
                                new FieldNode("startTime","long",null,new List<string>{"private"}),
                                new FieldNode("average","double",null,new List<string>{"private"})
                            },
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "Tests.Integration",
            new List<FileNode>
            {
                new FileNode(
                    "UserServiceTests.java",
                    "C:/Projects/ar-3d-visual-city/test/Tests/Integration/UserServiceTests.java",
                    "Tests.Integration",
                    2,
                    74,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "UserServiceTests",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            70,
                            3,
                            0,
                            new List<MethodNode>
                            {
                                new MethodNode("setup","void",new List<string>(),new List<string>(),new List<string>{"@Before","public"},12),
                                new MethodNode("testCreateUser","void",new List<string>(),new List<string>(),new List<string>{"@Test","public"},8),
                                new MethodNode("testFindUser","void",new List<string>(),new List<string>(),new List<string>{"@Test","public"},8)
                            },
                            new List<FieldNode>(),
                            new List<ClassNode>()
                        )
                    }
                )
            }
        ),
        new PackageNode(
            "Tests.Unit",
            new List<FileNode>
            {
                new FileNode(
                    "MathUtilsTests.java",
                    "C:/Projects/ar-3d-visual-city/test/Tests/Unit/MathUtilsTests.java",
                    "Tests.Unit",
                    2,
                    54,
                    new List<ClassNode>
                    {
                        new ClassNode(
                            "MathUtilsTests",
                            false,
                            false,
                            new List<string>{"public"},
                            "java.lang.Object",
                            new List<string>(),
                            50,
                            3,
                            0,
                            new List<MethodNode>
                            {
                                new MethodNode("testAdd","void",new List<string>(),new List<string>(),new List<string>{"@Test","public"},6),
                                new MethodNode("testSubtract","void",new List<string>(),new List<string>(),new List<string>{"@Test","public"},5),
                                new MethodNode("testMultiply","void",new List<string>(),new List<string>(),new List<string>{"@Test","public"},5)
                            },
                            new List<FieldNode>(),
                            new List<ClassNode>()
                        )
                    }
                )
            }
        )
    });
        dummy.BuildHierarchy();
        return dummy;
    }

    public void BuildHierarchy()
    {
        var packageDict = new Dictionary<string, PackageNode>();
        var topLevelPackages = new List<PackageNode>();

        foreach (var pkg in Packages)
            packageDict[pkg.Name] = pkg;

        foreach (var pkg in Packages)
        {
            if (string.IsNullOrEmpty(pkg.Name) || !pkg.Name.Contains('.'))
            {
                if (!topLevelPackages.Contains(pkg))
                    topLevelPackages.Add(pkg);
                continue;
            }

            var parentName = pkg.Name.Substring(0, pkg.Name.LastIndexOf('.'));

            if (!packageDict.TryGetValue(parentName, out var parent))
            {
                parent = new PackageNode(parentName, new List<FileNode>());
                packageDict[parentName] = parent;
                topLevelPackages.Add(parent);
            }

            parent.SubPackages ??= new List<PackageNode>();
            if (!parent.SubPackages.Contains(pkg))
                parent.SubPackages.Add(pkg);
        }

        Packages = topLevelPackages;
    }
}

    [Serializable]
public class PackageNode
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("files")]
    public List<FileNode> Files { get; set; }

    [JsonIgnore]
    public List<PackageNode> SubPackages { get; set; }

    public PackageNode(string name, List<FileNode> files)
    {
        Name = name;
        Files = files;
    }
}

[Serializable]
public class FileNode
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("packageName")]
    public string PackageName { get; set; }

    [JsonProperty("importCount")]
    public int ImportCount { get; set; }

    [JsonProperty("lineCount")]
    public int LineCount { get; set; }

    [JsonProperty("classes")]
    public List<ClassNode> Classes { get; set; }

    public FileNode(string name, string path, string packageName, int importCount, int lineCount, List<ClassNode> classes)
    {
        Name = name;
        Path = path;
        PackageName = packageName;
        ImportCount = importCount;
        LineCount = lineCount;
        Classes = classes;
    }
}

[Serializable]
public class ClassNode
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("isInterface")]
    public bool IsInterface { get; set; }

    [JsonProperty("isAbstract")]
    public bool IsAbstract { get; set; }

    [JsonProperty("modifiers")]
    public List<string> Modifiers { get; set; }

    [JsonProperty("superClass")]
    public string SuperClass { get; set; }

    [JsonProperty("interfaces")]
    public List<string> Interfaces { get; set; }

    [JsonProperty("lineCount")]
    public int LineCount { get; set; }

    [JsonProperty("methodCount")]
    public int MethodCount { get; set; }

    [JsonProperty("fieldCount")]
    public int FieldCount { get; set; }

    [JsonProperty("methods")]
    public List<MethodNode> Methods { get; set; }

    [JsonProperty("fields")]
    public List<FieldNode> Fields { get; set; }

    [JsonProperty("innerClasses")]
    public List<ClassNode> InnerClasses { get; set; }

    public ClassNode(
        string name,
        bool isInterface,
        bool isAbstract,
        List<string> modifiers,
        string superClass,
        List<string> interfaces,
        int lineCount,
        int methodCount,
        int fieldCount,
        List<MethodNode> methods,
        List<FieldNode> fields,
        List<ClassNode> innerClasses)
    {
        Name = name;
        IsInterface = isInterface;
        IsAbstract = isAbstract;
        Modifiers = modifiers;
        SuperClass = superClass;
        Interfaces = interfaces;
        LineCount = lineCount;
        MethodCount = methodCount;
        FieldCount = fieldCount;
        Methods = methods;
        Fields = fields;
        InnerClasses = innerClasses;
    }
}

[Serializable]
public class MethodNode
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("returnType")]
    public string ReturnType { get; set; }

    [JsonProperty("parameters")]
    public List<string> Parameters { get; set; }

    [JsonProperty("annotations")]
    public List<string> Annotations { get; set; }

    [JsonProperty("modifiers")]
    public List<string> Modifiers { get; set; }

    [JsonProperty("lineCount")]
    public int LineCount { get; set; }

    public MethodNode(string name, string returnType, List<string> parameters,
        List<string> annotations, List<string> modifiers, int lineCount)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Annotations = annotations;
        Modifiers = modifiers;
        LineCount = lineCount;
    }
}

[Serializable]
public class FieldNode
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("annotations")]
    public List<string> Annotations { get; set; }

    [JsonProperty("modifiers")]
    public List<string> Modifiers { get; set; }

    public FieldNode(string name = "", string type = "", List<string> annotations = null, List<string> modifiers = null)
    {
        Name = name;
        Type = type;
        Annotations = annotations ?? new List<string>();
        Modifiers = modifiers ?? new List<string>();
    }
}

[Serializable]
public class ExecutionSample
{
    [JsonProperty("timestamp")]
    public long TimeStamp { get; set; }

    [JsonProperty("frames")]
    public List<Frame> Frames { get; set; }

    [Serializable]
    public class Frame
    {
        [JsonProperty("className")]
        public string ClassName { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("line")]
        public int Line { get; set; }
    }
}
