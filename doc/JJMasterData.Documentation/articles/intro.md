<h1 align="center">
  <br>
<img width=15% src="../media/JJMasterDataLogoVertical.png"/>
</h1>

## Why did we create?
A long time ago, we created a product and started to implement it in some customers, but each one needed  different fields in its forms of specific customization rules.
First we replicated the code, one for each customer, with their specifications. <br>
I don't even need to say that this was the worst possible solution ever.<br>
So we mapped all the rules and had the brilliant idea of ​​parameterizing them within a single system and whether or not to enable it a certain field or functionality etc.<br>
Over time, we understood that this would be unfeasible, as each customer had a different particularity. Also the parameterization became complex and the end of this job wasn't visible.<br>
<br>
Next, we started to look for an existing solution in FOSS community and how the community solved it. So we find out the "Data Dictionary solution". <br>
That's how big ERP companies works!<br>
At a close look, you will realize that each ERP is customized to the needs of each company, and no one will ever be the same as the other, but the codebase is always the same.<br>
And each customer has the freedom to customize the system functionality and create its own fields, tables and even forms!<br>
<br>
We didn't find anything open source that allowed us to configure business fields and rules efficiently, so we thought we'd create our own. At first, we thought this was crazy.... but we are crazy! <br>
We made our own data dictionary and we liked it so much that we started using it on several systems.<br>

<p align="center">
<img alt="Do It Myself" src="../media/ThanosDoIt.gif"/>
</p>

On 09/09/2022 JJMasterData became part of the open-source community, where everyone can contribute to improve it.

## Why use? 
- Code Pattern<br>
  Improvements, updates and fixes without code repetition.

- Customize business rules<br>
  Each client can customize fields and business rules, using its own version control.

- Development speed with focus on quality<br>
  Create records quickly and securely, avoiding rework.

- CRUD at runtime instead of compile time<br>
  Enables runtime customization without having to recompile the system.


## When not to use?
- Complex screens that are not standard CRUD<br>
  When it involves great complexity and there is no need for customization in the future.

- Static contents pages<br>
  It makes no sense for static content.

## What is JJMasterData?
JJMasterData is an open-source .NET library to help you create CRUDs quickly from data dictionaries (database metadata), along with other boilerplate-intensive things like exporting and importing data.

## Who is using JJMasterData ?
JJMasterData is **production-ready** and is already being used by [JJConsulting](https://jjconsulting.tech).
