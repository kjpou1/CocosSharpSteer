CocosSharpSteer is a port of [Martin Even's SharpSteer2](https://github.com/martindevans/SharpSteer2) to CocosSharp.  

SharpSteer is a C# port of [OpenSteer](http://opensteer.sourceforge.net/). Like OpenSteer, the aim of SharpSteer is to help construct steering behaviors for autonomous characters in games and animation, with a current implementation focus toward Microsoft's XNA.

Like OpenSteer, CocosSharpSteer provides a CocosSharp application which demonstrates predefined steering behaviors. The user can quickly prototype, visualize, annotate and debug new steering behaviors by writing a plug-in for this Demo application.

This fork of CocosSharpSteer as ported from SharpSteer2 includes:

 - Modified to use Xamarin's [CocosSharp game engine](https://github.com/mono/CocosSharp)
 - Proper use of C# features such as extension methods to make the library easier to use.
 - Changes  to improve code quality/neatness.
 - Total separation of the demo and the library applications.
 - Some behaviours mentioned in the [original paper](http://www.red3d.com/cwr/papers/1999/gdc99steer.html) but never implemented in OpenSteer.

### Nuget

CocosSharpSteer will provide a NuGet package in the future.

### Documentation

* The original steering behaviours OpenSteer is documented [here](http://opensteer.sourceforge.net/)
* The original steering behaviours are documented [here](http://www.red3d.com/cwr/papers/1999/gdc99steer.html)

### Useful Books
The author of OpenSteer's site [ai-junkie](http://www.ai-junkie.com/books/toc_pgaibe.html)
