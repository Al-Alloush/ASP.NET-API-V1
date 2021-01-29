# Api Project

## This API Application for the basis of all API projects
this API use Identity Entity Framework Core Package to manage and store users.

when the user registered, an application sending an activation email to the user to Activate this Account, for this function I use SendGrid API. And send an SMS message when the user adds his phone number or update it, for this function I use Twilio API.

This Application is for Post Blogs, this Post is created just from Admins, but users can like or dislike these blogs and they can react with these blogs by comments.
Admins Create Blogs with many languages, default languages are: 
- "English", "العربية", "Deutsche", "Français", "Italiana", "español", "Nederlands", "svenska", "Türk"

Admins can add blogs’ categories with default languages too, they must add the parent category then add categories as children with any language with the Parent ID.
Users can select languages to display blogs with these languages
