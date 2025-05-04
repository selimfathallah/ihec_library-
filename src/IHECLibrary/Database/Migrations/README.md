# Database Migrations

This directory contains database migrations and storage setup scripts for the IHEC Library application.

## Storage Setup Instructions

The file `setup_profile_pictures_storage.sql` contains SQL statements to set up the profile pictures storage bucket in Supabase. 

To run these statements:

1. Log in to your Supabase dashboard
2. Go to the SQL Editor
3. Copy and paste the contents of the SQL file
4. Run the SQL statements

Note: These statements need to be run by a user with administrative privileges.

## Default Profile Picture

After setting up the storage bucket, upload a default profile picture:

1. In the Supabase dashboard, navigate to Storage
2. Select the 'profile-pictures' bucket
3. Upload a default profile picture with the name 'default-profile.png'
4. Make sure the file is publicly accessible

This default image will be used when users don't upload their own profile picture. 