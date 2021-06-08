#pragma once 
#include <Windows.h>
#include <iostream>

#include <string.h>

#include <libsoup\soup.h>
#include <libsoup\soup-websocket.h>
#include <glib-2.0\glib.h>
#include <libsoup\soup-types.h>
#include <json-glib\json-glib.h>

#include <glib.h>
#include <glib-object.h>
#include <gio/gio.h>

#include <stdio.h>
#include <stdlib.h>

/*
typedef int CakeFlavor;

typedef int CakeFrostingType;

typedef struct {
	guint radius;
	CakeFlavor flavor;
	CakeFrostingType frosting;
	char* message;
} CakeData;

typedef GObject Cake;

typedef GObject Baker;

static void cake_data_free(CakeData* cake_data)
{
	printf("%p: %s\n", g_thread_self(), __func__);

	g_free(cake_data->message);
	g_slice_free(CakeData, cake_data);
}

static Cake* bake_cake(Baker* self, guint radius, CakeFlavor flavor,
	CakeFrostingType frosting, char* message,
	GCancellable* cancellable, GError** error)
{
	printf("%p: %s\n", g_thread_self(), __func__);

	return g_object_new(G_TYPE_OBJECT, NULL);
}

static void bake_cake_thread(GTask* task, gpointer source_object,
	gpointer task_data, GCancellable* cancellable)
{
	Baker* self = source_object;
	CakeData* cake_data = task_data;
	Cake* cake;
	GError* error = NULL;

	printf("%p: %s\n", g_thread_self(), __func__);

	cake = bake_cake(self, cake_data->radius, cake_data->flavor,
		cake_data->frosting, cake_data->message, cancellable,
		&error);
	if (cake)
		g_task_return_pointer(task, cake, g_object_unref);
	else
		g_task_return_error(task, error);
}

static void baker_bake_cake_async(Baker* self, guint radius, CakeFlavor flavor,
	CakeFrostingType frosting,
	const char* message,
	GCancellable* cancellable,
	GAsyncReadyCallback callback,
	gpointer user_data)
{
	CakeData* cake_data;
	GTask* task;

	printf("%p: %s\n", g_thread_self(), __func__);

	cake_data = g_slice_new(CakeData);
	cake_data->radius = radius;
	cake_data->flavor = flavor;
	cake_data->frosting = frosting;
	cake_data->message = g_strdup(message);

	task = g_task_new(self, cancellable, callback, user_data);
	g_task_set_task_data(task, cake_data, (GDestroyNotify)cake_data_free);
	g_task_run_in_thread(task, bake_cake_thread);

	g_object_unref(task);
}

static Cake* baker_bake_cake_finish(Baker* self, GAsyncResult* res,
	GError** error)
{
	g_return_val_if_fail(g_task_is_valid(res, self), NULL);

	printf("%p: %s\n", g_thread_self(), __func__);

	return g_task_propagate_pointer(G_TASK(res), error);
}

static void my_callback(GObject* source_object, GAsyncResult* res,
	gpointer user_data)
{
	Baker* baker = (Baker*)source_object;
	GMainLoop* loop = (GMainLoop*)user_data;
	Cake* cake;
	GError* error = NULL;

	printf("%p: %s\n", g_thread_self(), __func__);

	cake = baker_bake_cake_finish(baker, res, &error);

	printf("A cake is baked: %p\n", cake);

	// But discard it. I prefer pudding.
	g_object_unref(cake);

	// Stop cooking.
	g_main_loop_quit(loop);
}

int main(void)
{
	Baker* baker = g_object_new(G_TYPE_OBJECT, NULL);
	GCancellable* cancellable = g_cancellable_new();

	GMainLoop* loop = g_main_loop_new(NULL, FALSE);

	baker_bake_cake_async(baker, 10, 20, 30, "emit", cancellable,
		my_callback, loop);
	g_object_unref(cancellable);

	printf("%p: start event loop.\n", g_thread_self());
	g_main_loop_run(loop);

	g_main_loop_unref(loop);
	g_object_unref(baker);

	return EXIT_SUCCESS;
}
*/