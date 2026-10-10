# Welcome to HandyUI documentation!

This is the homepage for HandyUI documentation.

You will get introduced to HandyUI and the reason to it's existence here.

## Features
- **Code-only** *(like WinForms)*
- Uses **[SkiaSharp](https://github.com/mono/skiasharp "SkiaSharp GitHub Repository")** for rendering
- **Open Source** and **free to modify**

## Why it exists

Basically, I *(davpnuser)* was trying to make a modern, hardware-accelerated GUI app in C# and modern .NET. I tried using WinForms, Uses GDI+. What about WPF? Uses XAML. Well, what about Avalonia? Nope, it has a lot of boilerplate. WinUI Markup? Sucks.

So, i was basically trying to find a option that didnt exist so at one point i got tired and started using SkiaSharp with WinForms. *(yes, i know, i was using the exact framework i didnt use at first.)* And it was going good until... I realized that WinForms is NOT cross-platform. So, i decided to try using alternatives suck as OpenTK's C# library and Silk.NET and etc.

But, ultimately, i found that Silk.NET works best. So, i was then using pure Silk.NET and SkiaSharp for a while until i realized once again, that, **there was a lot of boilerplate.** So, i began work on **HandyUI.** At first, it was really incapable. Really. Incapable. Then, over time, it started getting better with more and more updates.

My decision of seperating the core renderer to a seperate library especially helped me out when i wanted to make a Silk.NET adapter. I first made the Silk.NET adapter, and once it got stable enough, i deprecated the WinForms *(legacy)* adapter.

## What's next for HandyUI?

I can't tell. It has **way more** features than it used to have, but still, im a **solo developer.** Don't expect much advanced work from me. **I'm a human too**, after all. Still, **i will be maintaining this package as long as i physically can** until i either **get burnt out** or **i take a break.** But, you **should not** expect advanced built-in controls from me. That is your work. You can use a control library; Make a control library. Do what you want but dont expect advanced controls from me.

