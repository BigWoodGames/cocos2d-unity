using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace BBGamelib{
	public class CCTMXObject{
		public Dictionary<string, string> properties;		
		public string getProperty(string key){
			if (properties == null)
				return null;
			string value = null;
			if (properties.TryGetValue (key, out value))
				return value;
			else
				return null;
		}
	}

	public class CCTMXTile : CCTMXObject{
		public int gid;
		public int row;
		public int col;
		public string file;
		public Dictionary<string, string> sharedProperties;
		public List<Vector2> polygonPoints;

		public string getSharedProperty(string key){
			if (sharedProperties == null)
				return null;
			string value = null;
			if (sharedProperties.TryGetValue (key, out value))
				return value;
			else
				return null;
		}
		
		public string getPropertyFromObjectAndShared(string key){
			string value = getProperty (key);
			if (value == null)
				value = getSharedProperty (key);
			return value;
		}
	}

	public abstract class CCTMXLayer : CCTMXObject{
		public string name;
		public bool visiable;
	}

	public class CCTMXTiledLayer : CCTMXLayer{
		public CCTMXTile[][] tiles;
	}
	public class CCTMXObjectGroup : CCTMXLayer{
		public List<CCTMXTile> objects;
	}

	public class CCTMXMap : CCTMXObject{
		public string fileName;
		public int rows;
		public int cols;
		public int tileWidth;
		public int tileHeight;
		public CCTMXLayer[] layers;
		public Dictionary<int, string> gidToFiles;
		public Dictionary<int, Dictionary<string, string>> gidToTileProperties;
        public CCTMXTiledLayer getTiledLayer(string name){
			for(int i=0; i<layers.Length; i++){
				CCTMXLayer layer = layers[i];
				if(layer.name == name && layer is CCTMXTiledLayer)
                    return layer as CCTMXTiledLayer;
            }
            return null;
        }
		public CCTMXObjectGroup getObjectGroup(string name){
			for(int i=0; i<layers.Length; i++){
				CCTMXLayer layer = layers[i];
				if(layer.name == name && layer is CCTMXObjectGroup)
					return layer as CCTMXObjectGroup;
			}
			return null;
		}
	}


	public class CCTMXParser
	{
		public static CCTMXMap Parse(string file, NSDictionary tilesetCaches=null){
			string text = LoadText (file);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml (text);
			XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;

			// get mapWidth, mapHeight, tileWidth, tileHeight
			XmlNode mapNode = xmlDoc.DocumentElement;

			float version = float.Parse(mapNode.Attributes["version"].InnerText);
			if (!FloatUtils.EQ (version, 1.0f)) {
				CCDebug.Warning("cocos2d:CCTMXMap: Found {0} tmx file, but only 1.0 version was tested.", version);		
			}

			string dir = file.Replace (Path.GetFileName(file), "");

			CCTMXMap map = new CCTMXMap ();
			map.fileName = file;
			map.cols   =  int.Parse(mapNode.Attributes["width"].InnerText);
			map.rows  = int.Parse(mapNode.Attributes["height"].InnerText);
			map.tileWidth  = int.Parse(mapNode.Attributes["tilewidth"].InnerText);
			map.tileHeight = int.Parse(mapNode.Attributes["tileheight"].InnerText);

			Dictionary<int, string> gidToFiles = new Dictionary<int, string> (256);
			Dictionary<int, Dictionary<string, string>> gidToTileProperties = new Dictionary<int, Dictionary<string, string>> (256);
			List<CCTMXLayer> layers = new List<CCTMXLayer> (8);

			
			var enumerator = nodeList.GetEnumerator();
			while (enumerator.MoveNext()) {
				XmlNode nodeData = (XmlNode)enumerator.Current;
				if (nodeData.Name == "tileset") {
					ParseTileset(nodeData, gidToFiles, gidToTileProperties, dir, tilesetCaches);
				}
				else if (nodeData.Name == "layer") {
					CCTMXLayer layer = ParseLayer(nodeData, map.cols, map.rows, gidToFiles, gidToTileProperties);
					layers.Add(layer);
				}
				else if (nodeData.Name == "objectgroup") {
					CCTMXLayer layer = ParseObjectGroup(nodeData, map.cols, map.rows, map.tileWidth, map.tileHeight, gidToFiles, gidToTileProperties);
					layers.Add(layer);
				}
				else if(nodeData.Name == "properties"){
					if(map.properties == null)
						map.properties = new Dictionary<string, string>();
					ParseProperties(nodeData, map.properties);
				}
			}
			map.gidToFiles = gidToFiles;
			map.gidToTileProperties = gidToTileProperties;
			map.layers = layers.ToArray ();
			return map;
		}

		static CCTMXLayer ParseObjectGroup(XmlNode nodeData, int cols, int rows, int tileWidth, int tileHeight, Dictionary<int, string> gidToFiles, Dictionary<int, Dictionary<string, string>> gidToTileProperties){
			string name    = nodeData.Attributes["name"].InnerText;
			
			CCTMXObjectGroup layer = new CCTMXObjectGroup();
			layer.name = name;
			layer.objects = new List<CCTMXTile> ();

			layer.visiable = nodeData.Attributes["visible"] != null 
				&& nodeData.Attributes["visible"].InnerText == "0";
			
			XmlNodeList childNodes    = nodeData.ChildNodes;
			
			var enumerator = childNodes.GetEnumerator();
			while (enumerator.MoveNext()) {
				XmlNode childNode = (XmlNode)enumerator.Current;
				if(childNode.Name == "object"){
					float mapX   = float.Parse(childNode.Attributes["x"].InnerText);
					float mapY   = float.Parse(childNode.Attributes["y"].InnerText);
					int col = Mathf.RoundToInt(mapX / tileWidth);
					
					XmlAttribute gidAttr= childNode.Attributes["gid"];
					CCTMXTile tile = new CCTMXTile();
					tile.col = col;
					if(gidAttr!=null){
						int row = Mathf.RoundToInt(mapY / tileHeight) - 1;
						tile.row = row;
						tile.gid = int.Parse(gidAttr.InnerText);
						if(!gidToFiles.TryGetValue(tile.gid, out tile.file)){
							throw new KeyNotFoundException(string.Format("CCTMXParser: file key not found with tile gid={0}, grid=[{1}, {2}], pos=[{3:0},{4,0}]", tile.gid, col, row, mapX, mapY));
						}

						Dictionary<string, string> tileProperties = null;
						if(!gidToTileProperties.TryGetValue(tile.gid, out tileProperties)){
							tileProperties = null;
						}
						tile.sharedProperties = tileProperties;
					}else{	
						int row = Mathf.RoundToInt(mapY / tileHeight);
						tile.row = row;
						tile.gid = -1;
						XmlAttribute wAttr= childNode.Attributes["width"];
						XmlAttribute hAttr= childNode.Attributes["height"];
						tile.polygonPoints = new List<Vector2>();
						if(wAttr != null && hAttr != null){
							float w = float.Parse(wAttr.InnerText);
							float h = float.Parse(hAttr.InnerText);
							Vector2 p0 = new Vector2(0, 0);
							Vector2 p1 = new Vector2(w/tileWidth, 0);
							Vector2 p2 = new Vector2(w/tileWidth, h/tileHeight);
							Vector2 p3 = new Vector2(0, h/tileHeight);
							
							tile.polygonPoints.Add(p0);
							tile.polygonPoints.Add(p1);
							tile.polygonPoints.Add(p2);
							tile.polygonPoints.Add(p3);
						}else{
							XmlNode polylineNode = childNode.SelectSingleNode("polyline");
							if(polylineNode!=null){
								string text = polylineNode.Attributes["points"].InnerText;
								string[] pointStrs = text.Split(' ');
								for(int i=0; i<pointStrs.Length; i++){
									string pointStr = pointStrs[i];
									if(pointStr.Trim().Length==0){
										continue;
									}
									string[] pointParts = pointStr.Split(',');
									float x = float.Parse(pointParts[0]);
									float y = float.Parse(pointParts[1]);
									int pcol = Mathf.RoundToInt(x / tileWidth);
									int prow = Mathf.RoundToInt(y / tileHeight);
									tile.polygonPoints.Add(new Vector2(pcol, prow));
								}
							}else{
								NSUtils.Assert(false, "cocos2d:CCTMXParse: Unsupported shape found: {0}, {1}, {2}", name, Mathf.RoundToInt(mapY / tileHeight), col);
							}
						}
					}
					layer.objects.Add(tile);
					
					XmlNode propertiesNode = childNode.SelectSingleNode("properties");
					if(propertiesNode!=null){
						tile.properties = new Dictionary<string, string>();
						ParseProperties(propertiesNode, tile.properties);
					}
				}else if(childNode.Name == "properties"){
					if(layer.properties==null)
						layer.properties = new Dictionary<string, string>();
					ParseProperties(childNode, layer.properties);
				}
			} 
			return layer;
		}

		static CCTMXLayer ParseLayer(XmlNode nodeData, int cols, int rows, Dictionary<int, string> gidToFiles, Dictionary<int, Dictionary<string, string>> gidToTileProperties){
			XmlNode data    = nodeData.SelectSingleNode("data");
			string name    = nodeData.Attributes["name"].InnerText;
			string encoding    = data.Attributes["encoding"].InnerText;
			NSUtils.Assert (encoding=="csv", "cocos2d:CCTMXMapParser: Unsupported encoding {0} found. Only csv is supported now.", encoding);

			string  csvData = data.InnerText;
			CCTMXTiledLayer layer = new CCTMXTiledLayer();
			layer.name = name;
			layer.visiable = nodeData.Attributes["visible"] != null && nodeData.Attributes["visible"].InnerText == "0";
			layer.tiles = new CCTMXTile[rows][];
			for (int row=0; row<rows; row++) {
				layer.tiles[row] = new CCTMXTile[cols];			
			}
			string[] layerData = csvData.Split(',');
			int totalTiles    = cols * rows;
			for (int i = 0; i < totalTiles; i++) {
				int col = i % cols;
				int row = Mathf.FloorToInt(i / cols);
				int tileId = int.Parse(layerData[i].ToString().Trim());
				if(tileId>0){
					CCTMXTile tile = new CCTMXTile();
					tile.gid = tileId;
					tile.col = col;
					tile.row = row;
					try{
						tile.file = gidToFiles[tile.gid];
					}catch{
						throw new UnityEngine.UnityException(string.Format("cocos2d: CCTMXMapParser: gid [{0}] not found", tile.gid));
					}
					Dictionary<string, string> tileProperties = null;
					if(!gidToTileProperties.TryGetValue(tile.gid, out tileProperties)){
						tileProperties = null;
					}
					tile.sharedProperties = tileProperties;
					layer.tiles[row][col] = tile;
				}
			} 

			XmlNode propertiesNode = nodeData.SelectSingleNode("properties");
			if(propertiesNode!=null){
				layer.properties = new Dictionary<string, string>();
				ParseProperties(propertiesNode, layer.properties);
			}
			return layer;
		}

		static void ParseProperties(XmlNode propertiesNode, Dictionary<string, string> properties){
			XmlNodeList propertyList = propertiesNode.SelectNodes("property");
			var enumerator = propertyList.GetEnumerator();
			while (enumerator.MoveNext()) {
				XmlNode propertyNode = (XmlNode)enumerator.Current;
				string key = propertyNode.Attributes["name"].InnerText;
				string value = propertyNode.Attributes["value"].InnerText;
				properties.Add(key, value);
			}
		}

		static void ParseTileset(XmlNode nodeData, Dictionary<int, string> gidToFiles, Dictionary<int, Dictionary<string, string>> gidToTileProperties, string dir, NSDictionary tilesetCaches){
			XmlNode source = nodeData.SelectSingleNode("image");
			
			int firstGid        = int.Parse(nodeData.Attributes["firstgid"].InnerText);
			int tileWidth  = int.Parse(nodeData.Attributes["tilewidth"].InnerText);
			int tileHeight = int.Parse(nodeData.Attributes["tileheight"].InnerText);
			
			string filename  = source.Attributes["source"].InnerText;
			string ext = Path.GetExtension(filename);
			if(ext!=null)
				filename = filename.Replace (ext, "");
			filename = dir + filename;
			ParseImagePlist(firstGid, tileWidth, tileHeight, filename, gidToFiles, tilesetCaches);
			
			//tile shared properties
			XmlNodeList tileNodes = nodeData.SelectNodes("tile");
			
			var enumerator = tileNodes.GetEnumerator();
			while (enumerator.MoveNext()) {
				XmlNode tileNode = (XmlNode)enumerator.Current;
				string id = tileNode.Attributes["id"].InnerText;
				if(id!=null && id.Trim().Length>0){
					int idInt = int.Parse(id);
					XmlNode propertiesNode = tileNode.SelectSingleNode("properties");
					if(propertiesNode!=null){
						XmlNodeList sharedPropertyNodes = propertiesNode.SelectNodes("property");
						Dictionary<string, string> sharedProperties = new Dictionary<string, string>();
						
						var sharedPropertyNodesEnumerator = sharedPropertyNodes.GetEnumerator();
						while (sharedPropertyNodesEnumerator.MoveNext()) {
							XmlNode sharedPropertyNode = (XmlNode)sharedPropertyNodesEnumerator.Current;
							string name = sharedPropertyNode.Attributes["name"].InnerText;
							string value = sharedPropertyNode.Attributes["value"].InnerText;
							if(name!=null && value != null){
								sharedProperties[name] = value;
							}
						}
						gidToTileProperties[idInt + firstGid] = sharedProperties;
					}
				}
			}
		}

		static void ParseImagePlist(int firstGid, int tileWidth, int tileHeight, string filename, Dictionary<int, string> gidToFiles, NSDictionary tilesetCaches){
			string path = filename + ".txt";
			NSDictionary plist = null;
			if(tilesetCaches!=null)
				plist = tilesetCaches.objectForKey<NSDictionary>(path);
			if(plist==null)
				plist = NSDictionary.DictionaryWithContentsOfFileFromResources(filename + ".txt");
			NSDictionary metaDataDict = plist.objectForKey<NSDictionary>("metadata");
			NSDictionary framesDict = plist.objectForKey<NSDictionary>("frames");
			int format = 0;
			if (metaDataDict != null) {
				format = metaDataDict.objectForKey<int> ("format");
			}

			int width = 0;
			if (metaDataDict != null) {
				Vector2 size = ccUtils.PointFromString ((string)metaDataDict ["size"]);	
				width = Mathf.RoundToInt(size.x);
			} else {
				NSDictionary texture = metaDataDict.objectForKey<NSDictionary>("texture");
				width = texture.objectForKey<int>("width");
			}
			
			var enumerator = framesDict.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<object, object> frameDictKeyValue = enumerator.Current;
				string frameDictKey = (string)frameDictKeyValue.Key;
				NSDictionary frameDict = (NSDictionary)frameDictKeyValue.Value;
				int x=0, y=0, w=0, h=0;
				if(format == 0) {
					float ox = frameDict.objectForKey<float>("x");
					float oy = frameDict.objectForKey<float>("y");
					float ow = frameDict.objectForKey<float>("width");
					float oh = frameDict.objectForKey<float>("height");
					x = Mathf.RoundToInt(ox);
					y = Mathf.RoundToInt(oy);
					w = Mathf.RoundToInt(ow);
					h = Mathf.RoundToInt(oh);
				} else if(format == 1 || format == 2) {
					Rect frame = ccUtils.RectFromString(frameDict.objectForKey<string>("frame"));
					x = Mathf.RoundToInt(frame.x);
					y = Mathf.RoundToInt(frame.y);
					w = Mathf.RoundToInt(frame.width);
					h = Mathf.RoundToInt(frame.height);
				} else if(format == 3) {
					Rect frame = ccUtils.RectFromString(frameDict.objectForKey<string>("textureRect"));
					x = Mathf.RoundToInt(frame.x);
					y = Mathf.RoundToInt(frame.y);
					w = Mathf.RoundToInt(frame.width);
					h = Mathf.RoundToInt(frame.height);
				}else{
					NSUtils.Assert(false, "cocos2d:CCTMXMap: Uknown TexturePack format.");
				}
				NSUtils.Assert(w==tileWidth && h==tileHeight, "cocos2d:CCTMXMap: Frame size of tileset file must be same with tmx tilesize.");
				int col = Mathf.RoundToInt(x / tileWidth);
				int row = Mathf.RoundToInt(y / tileHeight);
				int cols = Mathf.RoundToInt(width / tileWidth); 
				int gid = firstGid + col + row * cols;
				gidToFiles[gid] = frameDictKey;
			}	
		}

		static string LoadText(string file){
			NSUtils.Assert (file.EndsWith (".txt"), "Text file in Resources folder must be '*.txt' format!");
			string ext = Path.GetExtension(file);
			file = file.Replace (ext, "");
			TextAsset asset = Resources.Load<TextAsset> (file);
			NSUtils.Assert (asset!=null, "cocos2d: CCTmxParser:File not found :{0}.", file);
			return asset.text;
		}


		//DEBUG
		public static void Debug(CCTMXMap map){
//			string s = "[TMX:" + map.fileName + "]\n";
//			if (map.properties != null) {
//				s += "  " + "Properties: ";
//				var enumerator = map.properties.GetEnumerator();
//				while (enumerator.MoveNext()) {
//					KeyValuePair<object, object> propPair = enumerator.Current;
//					s += propPair.Key + "=" + propPair.Value + " ";
//				}	
//				s += "\n";
//			}
//			s += "  " + "GID-FILES:\n";
//			foreach (KeyValuePair<int, string> pair in map.gidToFiles) {
//				s += "  " + "  " + pair.Key + "=" + pair.Value;	
//				if(map.gidToTileProperties.ContainsKey(pair.Key)){
//					s += " properties: "; 
//					var enumerator = map.gidToTileProperties[pair.Key].GetEnumerator();
//					while (enumerator.MoveNext()) {
//						KeyValuePair<object, object> propPair = enumerator.Current;
//						s += propPair.Key + "=" + propPair.Value + " ";
//					}
//				}
//				s += "\n";
//			}
//			s +=	"  " + "Layers("+ map.layers.Length +")\n";
//			foreach (CCTMXLayer layer in map.layers) {
//				s += "  " + "  " + "Layer:" + layer.name + "\n";
//				if (layer.properties != null) {
//					s += "  " + "  " + "  " + "Properties: ";
//					
//					var enumerator = layer.properties.GetEnumerator();
//					while (enumerator.MoveNext()) {
//						KeyValuePair<object, object> propPair = enumerator.Current;
//						s += propPair.Key + "=" + propPair.Value + " ";
//					}	
//					s += "\n";
//				}
//				s += "  " + "  " + "  " + "Tiles:\n";
//				for(int row=0; row<layer.tiles.Length; row++){
//					s += "  " + "  " + "  " + " ";
//					for(int col=0; col<layer.tiles[row].Length; col++)
//					{
//						CCTMXTile tile = layer.tiles[row][col];
//						if(tile!=null)
//							s += layer.tiles[row][col].gid + " ";
//						else
//							s += "0" + " ";
//					}
//					s += "\n";
//				}
//			}
//			CCDebug.Log (s);
		}
	}
}

