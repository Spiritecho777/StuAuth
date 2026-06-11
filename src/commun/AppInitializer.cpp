#include "AppInitializer.h"
#include "LockManager.h"
#include "CryptoInitializer.h"

bool AppInitializer::init()
{
	CryptoInitializer::init();
	return LockManager::acquire();
}