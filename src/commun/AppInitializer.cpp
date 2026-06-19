#include "AppInitializer.h"
#include "security/LockManager.h"
#include "security/CryptoInitializer.h"
#include "security/AuthManager.h"

bool AppInitializer::init()
{
	CryptoInitializer::init();

	if (!LockManager::acquire()) return false;
	if (!AuthManager::init()) return false;
	
	return true;
}