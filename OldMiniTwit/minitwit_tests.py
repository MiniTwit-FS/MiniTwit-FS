# -*- coding: utf-8 -*-
"""
    MiniTwit Tests
    ~~~~~~~~~~~~~~

    Tests the MiniTwit application.

    :copyright: (c) 2010 by Armin Ronacher.
    :license: BSD, see LICENSE for more details.
"""
import unittest
import tempfile
import minitwitdeepseek
import os
import unittest
import requests

class MiniTwitTestCase(unittest.TestCase):


# Get the current script directory

# Set the database file to be in the same folder as the script
    BASE_URL = "http://localhost:5244"  # Adjust based on your C# API


    def setUp(self):
        """Before each test, set up a blank database"""
        #self.db = tempfile.NamedTemporaryFile()
        #self.app = minitwitdeepseek.app.test_client()
        #minitwitdeepseek.DATABASE = tempfile.mktemp()
        #minitwitdeepseek.init_db()
        self.session = requests.Session()
        self.get("/drop/all")

    def post(self, endpoint, data, follow_redirects = False):
        """Helper function to send POST requests"""
        return self.session.post(f"{self.BASE_URL}{endpoint}", json=data, allow_redirects=follow_redirects)


    def get(self, endpoint, follow_redirects = False):
        """Helper function to send GET requests"""
        return self.session.get(f"{self.BASE_URL}{endpoint}", allow_redirects=follow_redirects)

    # helper functions

    def register(self, username, password, password2=None, email=None):
        """Helper function to register a user"""
        if password2 is None:
            password2 = password
        if email is None:
            email = username + '@example.com'
        return self.post('/register', data={
            'username':     username,
            'password':     password,
            'password2':    password2,
            'email':        email,
        }, follow_redirects=True)

    def login(self, username, password):
        """Helper function to login"""
        return self.post('/login', data={
            'username': username,
            'password': password
        }, follow_redirects=True)

    def register_and_login(self, username, password):
        """Registers and logs in in one go"""
        self.register(username, password)
        return self.login(username, password)

    def logout(self):
        """Helper function to logout"""
        return self.get('/logout', follow_redirects=True)

    def add_message(self, text):
        """Records a message"""
        rv = self.post('/add_message', data={'text': text},
                                    follow_redirects=True)
        if text:
            assert 'Your message was recorded' in rv.text
        return rv

    # testing functions

    def test_register(self):
        """Make sure registering works"""
        rv = self.register('user1', 'default')
        assert 'You were successfully registered ' \
               'and can login now' in rv.text
        rv = self.register('user1', 'default')
        assert 'The username is already taken' in rv.text
        rv = self.register('', 'default')
        assert 'You have to enter a username' in rv.text
        rv = self.register('meh', '')
        assert 'You have to enter a password' in rv.text
        rv = self.register('meh', 'x', 'y')
        assert 'The two passwords do not match' in rv.text
        rv = self.register('meh', 'foo', email='broken')
        assert 'You have to enter a valid email address' in rv.text

    def test_login_logout(self):
        """Make sure logging in and logging out works"""
        rv = self.register_and_login('user1', 'default')
        assert 'You were logged in' in rv.text
        rv = self.logout()
        assert 'You were logged out' in rv.text
        rv = self.login('user1', 'wrongpassword')
        assert 'Invalid password' in rv.text
        rv = self.login('user2', 'wrongpassword')
        assert 'Invalid username' in rv.text

    def test_message_recording(self):
        """Check if adding messages works"""
        self.register_and_login('foo', 'default')
        self.add_message('test message 1')
        self.add_message('<test message 2>')
        rv = self.get('/')
        assert 'test message 1' in rv.text
        assert '<test message 2>' in rv.text

    def test_timelines(self):
        """Make sure that timelines work"""
        self.register_and_login('foo', 'default')
        self.add_message('the message by foo')
        self.logout()
        self.register_and_login('bar', 'default')
        self.add_message('the message by bar')
        rv = self.get('/public')
        assert 'the message by foo' in rv.text
        assert 'the message by bar' in rv.text

        # bar's timeline should just show bar's message
        rv = self.get('/')
        assert 'the message by foo' not in rv.text
        assert 'the message by bar' in rv.text

        # now let's follow foo
        rv = self.get('/foo/follow', follow_redirects=True)
        assert 'You are now following foo' in rv.text

        # we should now see foo's message
        rv = self.get('/')
        
        assert 'the message by foo' in rv.text
        assert 'the message by bar' in rv.text

        # but on the user's page we only want the user's message
        rv = self.get('/bar')
        assert 'the message by foo' not in rv.text
        assert 'the message by bar' in rv.text
        rv = self.get('/foo')
        assert 'the message by foo' in rv.text
        assert 'the message by bar' not in rv.text

        # now unfollow and check if that worked
        rv = self.get('/foo/unfollow', follow_redirects=True)
        assert 'You are no longer following foo' in rv.text
        rv = self.get('/')
        assert 'the message by foo' not in rv.text
        assert 'the message by bar' in rv.text


if __name__ == '__main__':
    unittest.main()
