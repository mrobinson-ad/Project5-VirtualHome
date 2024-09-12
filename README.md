# Virtual Home
The fifth project in the MakeYourGame online XR App developper course is to make a furniture store app with AR previewing capabilities.
Please note that as my device doesn't support AR depth perception the AR placement is not always at ground level in the showcased pictures and gifs.


### The concept of the App
For this app I wanted the design to be sobre and intuitive and bring emphasis on the products being sold, this is the reasoning behind the color scheme being a gray scale.
<br>
If you want more insight on the creative process and the research behind the project feel free to check out the [product specifications document](/GitAssets/ProductSpec.pdf).

<br>

## Vuforia
The AR SDK chosen for this project is Vuforia. It's ease of use and robust off screen tracking features are the main appeal for this project considering we do not need to use any of its premium features. For a more in depth look into the SDK and its alternative feel free to check the product specifications part II.E Technical specifications.
<details>
<summary>Click here to see more details on the AR</summary>
  
> Placing an object and removing it
![Clicking on a button to preview a transparent version of the object, placing it and then removing it with the trash icon in the information panel](/GitAssets/)

> Snippet of code that replaces the default Vuforia behaviors
```c#
public void PlaceGround(HitTestResult hit)
    {
        if (anchorStage.name != "Base Plane")
        {
            anchorStage = VuforiaBehaviour.Instance.ObserverFactory.CreateAnchorBehaviour("Base Plane", hit);
        }

        if (isPlacing)
        {
            PlaceNewObject(hit, objectToPlace);
        }
    }
```
> Changing an object's color
![Changing an object's color by clicking a button in the information panel](/GitAssets/)

> By using both interactive and automatic hit tests we can provide a dynamic preview and a user driven placement
![The plane finder object in inspector](/GitAssets/PlaneFinder.PNG)

</details>

## Screenshots and gifs
<details>
<summary>Click here to view some images along with their explanations</summary>

> Adding to favorites and changing colors
> 
![Adding a product to favorites from the product page and changing its color](/GitAssets/ProductPage.gif)

> Searching for products by category or name
>     
![Clicking on a category to view the related products and searching products by name](/GitAssets/Search.gif)

> Dynamic password security checker
>     
![Conditions to meet password security turning from red to green as we achieve them](/GitAssets/Password.gif)

</details>

<br>

# The features of the app

<br>
 
- Unity addressables:
    -
    By using Unity addressables and Unity Cloud content Delivery we can store the 3D models and scriptable objects of the furniture with Unity cloud services in order to deliver a lighter app while allowing the addition of new products without a need to update the app directly.

- UIToolkit:
    -
    The app is built using UIToolkit for a lightweight and easily editable UI. By using USS classes and transitions we can easily change elements at runtime and create simple but smooth animation. We make full use of templates by instantiating all elements related to products at runtime to easily accomodate new additions.
  
- Scriptable Objects
    -
    As mentioned before all the products are stored as scriptable objects. This allows us to easily create, edit and delete various information about each product including dynamic variables like whether they are on sale or not.
  
- User Tab
    -
    While most of the user tab features are built for design and developement purposes (The created user being stored in playerprefs) the idea behind it is to provide quality of life features for users by saving their favorites, saving their shipping address and having access to their purchase history and ongoing orders. These features will be expanded upon in a further project where we will work on databases.

- LeanTouch
    -
    To facilitate the touch input interactions I use Lean touch for tapping, rotating and dragging models in the AR preview.

