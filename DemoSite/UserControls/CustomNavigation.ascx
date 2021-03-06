﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CustomNavigation.ascx.cs" Inherits="DemoSite.UserControls.CustomNavigation" %>

<div class="collapse navbar-collapse" id="ftco-nav">
    <ul class="navbar-nav ml-auto">
        <li class="nav-item active"><a href="index.html" class="nav-link">Home</a></li>
        <li class="nav-item"><a href="about.html" class="nav-link">About</a></li>
        <li class="nav-item"><a href="services.html" class="nav-link">Services</a></li>
        <li class="nav-item dropdown">
            <a class="nav-link dropdown-toggle" href="portfolio.html" id="dropdown04" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">Portfolio</a>
            <div class="dropdown-menu" aria-labelledby="dropdown04">
                <a class="dropdown-item" href="portfolio.html">Portfolio</a>
                <a class="dropdown-item" href="portfolio-single.html">Portfolio Single</a>
            </div>
        </li>
        <li class="nav-item"><a href="blog.html" class="nav-link">Case Studies</a></li>
        <li class="nav-item"><a href="contact.html" class="nav-link">Contact</a></li>
        <li class="nav-item cta"><a href="contact.html" class="nav-link"><span>Get in touch</span></a></li>
    </ul>
</div>