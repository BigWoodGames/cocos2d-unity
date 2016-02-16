## Getting Started

1. First, you'll need to download the cocos2d-unity plug-in.
    ```
    $ git clone --recursive git@github.com:BigWoodGames/cocos2d-unity.git
    ```


2. Create a new **2D project** in the Unity Editor.

    >Unity 5 is required. Unity 5.1.2f1 is our recommended version.

  ![alt text](http://www.bigwoodgames.com/web/images/dev/step2.jpg "New Project")

3. Drag the **BBGamelib** folders into your **Assets** path:

  ![alt text](http://www.bigwoodgames.com/web/images/dev/step3.jpg "Add plugin")

4. Create a c# script **AppDelegate.cs** under **Assets/Scripts**.

  ![alt text](http://www.bigwoodgames.com/web/images/dev/step4.jpg "Add plugin")

5. Double click AppDelegate.cs script to open MonoDevelop.

  ![alt text](http://www.bigwoodgames.com/web/images/dev/step5.jpg "AppDelegate")
  
 Add the following code to the class:
 
``` cs
using UnityEngine;
using System.Collections;
using BBGamelib;

public class AppDelegate : CCAppDelegate {
	
    //It's expansive to create a component during runtime in Unity.  
    //So cocos2d-unity implements a components pool which created in editor mode. 
    public override void applicationRunOnceOnBuilding ()
    {
        base.applicationRunOnceOnBuilding ();
        
        //add components need to pool
        //set pool size of CCNode.
        _factory.generateNodeGearsInEditMode(1024);
        
        //set pool size of CCSprite
        _factory.generateSpriteGearsInEditMode(1024);
        
        //set poool size of CCLabel
        _factory.generateLabelGearsInEditMode(16);
    }
    
    //Same as you always do in xcode.  Called after the application loaded.
    public override void applicationDidFinishLaunching ()
    {
        //Set the resolution height.  Screen will autosize to fit the height. 
        //You can set different height for different devices.
        _window.setResolutionHeight (640);
        
        _view.setFrame (_window.bounds);
        CCDirector.Reset ();
        _director = CCDirector.sharedDirector;
        _director.displayError = true;
        _director.displayStats = true;
        _director.displayLink = this;
        _director.animationInterval = 1.0f / 60;
        _director.view = _view;
        _window.rootViewController = _director;
        
        //Set your scene
        _director.presentScene(HelloWorldLayer.Scene ());
    }
     
    //Called when application quit
    void OnApplicationQuit() {
    }

    //Called when application enter background or resume
    void OnApplicationPause(bool paused)
    {
    }
}
			
```

6. Create a empty **GameObject** under **Hierarchy**:

  ![alt text](http://www.bigwoodgames.com/web/images/dev/step6-1.jpg "GameObject")
  
  **Add Component** to the **GameObject**, and choose **Scripts > App Delegate**:
  
  ![alt text](http://www.bigwoodgames.com/web/images/dev/step6-2.jpg "GameObject")
 
7. Create a c# script **HelloWorldLayer.cs** derived from **CCLayer** Add the following method to the class:

``` cs           
public class HelloWorldLayer : CCLayer {
    // Helper class method that creates a Scene with the HelloWorldLayer as the only child.
    public static CCScene  Scene()
    {
        // 'scene' is an autorelease object.
        CCScene scene = new CCScene();
        
        // 'layer' is an autorelease object.
        HelloWorldLayer layer = new HelloWorldLayer();
        
        // add layer as a child to scene
        scene.addChild(layer);
        
        // return the scene
        return scene;
    }
    // on "init" you need to initialize your instance
    protected override void init ()
    {
        base.init ();

        // create and initialize a Label
        CCLabelTTF label = new CCLabelTTF("Hello World","Arial", 64);
        
        // ask director for the window size 
        Vector2 size = CCDirector.sharedDirector.winSize;
        
        // position the label on the center of the screen
        label.position = size / 2;
        
        // add the label as a child to this Layer
        addChild(label);

        
        //
        // menu items
        //
        
        // Default font size will be 28 points.
        CCMenuItemFont.FontSize = 28;
        
        // Cocos2d Menu Item using blocks
        CCMenuItem itemCocos2d = new CCMenuItemFont("Cocos2d", delegate(object sender) {
            label.text = "Hello Cocos2d";                               
        });
        
        // Unity Menu Item using blocks
        CCMenuItem itemUnity = new CCMenuItemFont("Unity", delegate(object sender) {
            label.text = "Hello Unity";                               
        });
        
        
        CCMenu menu = new CCMenu(itemCocos2d, itemUnity);

        menu.alignItemsHorizontallyWithPadding(20);
        menu.position = new Vector2(size.x/2, size.y/2 - 50);
        
        // Add the menu to the layer
        addChild(menu);
    }
}
```

That's it! You now can code your game like using cocos2d-objc in xcode!

  ![alt text](http://www.bigwoodgames.com/web/images/dev/step8.jpg "GameObject")
.

+ [Framework Website](http://www.bigwoodgames.com/preview/developer.php)

